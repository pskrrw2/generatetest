using Application.Dto;
using Application.ICurrentUserService;
using Application.IDataService;
using Application.Service;
using Domain.Common.Const;
using Domain.Common.Enums;
using Domain.Entities;

namespace Infrastructure.DataService;

public class ConferenceService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : IConferenceService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    public async Task<bool> AddAsync(ConferenceDto conferenceDto)
    {
        try
        {
            var conference = ConferenceMapping(conferenceDto);

            await _unitOfWork.GenericRepository<Conference>().AddAsync(conference);
            await _unitOfWork.SaveAsync();

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task DeleteById(Conference conference)
    {
        _unitOfWork.GenericRepository<Conference>().Delete(conference);
        await _unitOfWork.SaveAsync();
    }

    public async Task<IEnumerable<Conference>> GetAllAsync()
    {
        return await _unitOfWork.GenericRepository<Conference>().GetAllAsync();
    }

    public async Task<IEnumerable<Conference>> GetByUserIdAsync(string userId)
    {
        var conferences = await GetAllAsync();
        return conferences.Where(x => x.CreatedBy == userId);
    }

    public async Task<Conference> GetById(object id)
    {
        return await _unitOfWork.GenericRepository<Conference>().GetByIdAsync(id);
    }

    public async Task<bool> Update(ConferenceDto conferenceDto)
    {
        try
        {
            var conference = await GetById(conferenceDto.Id);
            if (conference is null)
                throw new ArgumentException($"Conference with ID {conferenceDto.Id} not found.");

            conference.AdminConferenceNotes = conferenceDto.AdminConferenceNotes;
            conference.LastModifiedOn = DateTimeOffset.UtcNow;
            conference.LastModifiedBy = _currentUserService.UserId;

            conference.CreatedBy = !_currentUserService.IsAdmin
                                 ? _currentUserService.UserId
                                 : conferenceDto.CreatedBy;
            conference.ConferenceId = conferenceDto.Id;
            conference.SuiteType = conferenceDto.SuiteType!;
            conference.ConferenceDate = (DateTimeOffset)Constants.ConvertToUtc(conferenceDto.ConferenceDate)!;
            conference.ConferenceNotes = conferenceDto.ConferenceNotes!;
            conference.Status = (_currentUserService.IsAdmin || conferenceDto.Status == (nameof(StatusType.Approved)))
                                 ? conferenceDto.Status
                                 : nameof(StatusType.Pending);
            conference.UserId = !_currentUserService.IsAdmin
                                 ? _currentUserService.UserId
                                 : conferenceDto.UserId;
            conference.PackageSelected = conferenceDto.PackageSelected;
            conference.PackageDate = !string.IsNullOrEmpty(conference.PackageSelected) ? DateTimeOffset.UtcNow : null;
            conference.AddonsSelected = conferenceDto.AddonsSelected;
            conference.AddOnDate = !string.IsNullOrEmpty(conference.AddonsSelected) ? DateTimeOffset.UtcNow : null;
            conference.ApproveRejectDate = (conference.ApproveRejectDate != null) ? conference.ApproveRejectDate : (_currentUserService.IsAdmin && conferenceDto.Status != nameof(StatusType.Pending)) ? DateTimeOffset.UtcNow : null;

            _unitOfWork.GenericRepository<Conference>().Update(conference);
            await _unitOfWork.SaveAsync();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private Conference ConferenceMapping(ConferenceDto conferenceDto)
    {
        return new Conference
        {
            ConferenceId = conferenceDto.Id,
            SuiteType = conferenceDto.SuiteType!,
            ConferenceDate = (DateTimeOffset)Constants.ConvertToUtc(conferenceDto.ConferenceDate)!,
            ConferenceNotes = conferenceDto.ConferenceNotes,
            CreatedOn = DateTimeOffset.UtcNow!,
            Status = nameof(StatusType.Pending),
            AdminConferenceNotes = conferenceDto.AdminConferenceNotes,
            CreatedBy = _currentUserService.UserId,
            UserId = _currentUserService?.UserId,
            PackageSelected = conferenceDto.PackageSelected,
            AddonsSelected = conferenceDto.AddonsSelected
        };
    }
}
