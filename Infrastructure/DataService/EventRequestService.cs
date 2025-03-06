using Application.Dto;
using Application.ICurrentUserService;
using Application.IDataService;
using Application.Service;
using Domain.Common.Enums;
using Domain.Entities;

namespace Infrastructure.DataService;

public class EventRequestService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : IEventRequestService
{

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    public async Task<bool> AddAsync(EventRequestDto eventRequestDto)
    {
        try
        {
            eventRequestDto.Status = nameof(StatusType.Pending);
            var eventRequest = EventRequestMapping(eventRequestDto);

            await _unitOfWork.GenericRepository<EventRequest>().AddAsync(eventRequest.Result);
            await _unitOfWork.SaveAsync();

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task DeleteById(EventRequest eventRequest)
    {
        _unitOfWork.GenericRepository<EventRequest>().Delete(eventRequest);
        await _unitOfWork.SaveAsync();
    }

    public async Task<IEnumerable<EventRequest>> GetAllAsync()
    {
        return await _unitOfWork.GenericRepository<EventRequest>().GetAllAsync();
    }

    public async Task<IEnumerable<EventRequest>> GetByUserIdAsync(string userId)
    {
        var eventRequests = await _unitOfWork.GenericRepository<EventRequest>().GetAllAsync();
        return eventRequests.Where(x => x.CreatedBy == userId);
    }

    public async Task<IEnumerable<EventRequest>> GetByEventIdAsync(int eventId)
    {
        var eventRequests = await _unitOfWork.GenericRepository<EventRequest>().GetAllAsync();
        return eventRequests.Where(x => x.EventId == eventId);
    }

    public async Task<EventRequest> GetById(object id)
    {
        return await _unitOfWork.GenericRepository<EventRequest>().GetByIdAsync(id);
    }

    public async Task<bool> Update(EventRequestDto eventRequestDto)
    {
        try
        {
            var eventRequest = await GetById(eventRequestDto.RequestId);
            if (eventRequest is null)
                throw new ArgumentException($"EventRequest with ID {eventRequestDto.RequestId} not found.");


            eventRequest.ApprovedParkingPasses = eventRequestDto.ApprovedParkingPasses;
            eventRequest.ApprovedTickets = eventRequestDto.ApprovedTickets;
            eventRequest.AdminNotes = eventRequestDto.AdminNotes;

            eventRequest.EventId = eventRequestDto.EventId ?? eventRequest.EventId;
            eventRequest.RequestId = eventRequestDto.RequestId;
            eventRequest.AppliedParkingPasses = eventRequestDto.AppliedParkingPasses!;
            eventRequest.ExecutiveNotes = eventRequestDto.ExecutiveNotes;
            eventRequest.AppliedTickets = eventRequestDto.AppliedTickets!;
            eventRequest.SROTickets = eventRequestDto.SROTickets;
            eventRequest.CateringAndDrinks = eventRequestDto.CateringAndDrinks;
            eventRequest.CreatedBy = !_currentUserService.IsAdmin
            ? _currentUserService.UserId
            : eventRequestDto.CreatedBy;
            eventRequest.LastModifiedBy = _currentUserService.UserId;

            eventRequest.PackageSelected = eventRequestDto.SelectedPackage;
            eventRequest.PackageDate = !string.IsNullOrEmpty(eventRequestDto.SelectedPackage)  ? DateTimeOffset.UtcNow : null;
            eventRequest.AddonsSelected = eventRequestDto.AddonsSelected;
            eventRequest.AddOnDate = !string.IsNullOrEmpty(eventRequestDto.AddonsSelected) ? DateTimeOffset.UtcNow : null;

            eventRequest.LastModifiedOn = DateTimeOffset.UtcNow;
            eventRequest.Status = (_currentUserService.IsAdmin || eventRequestDto.Status == (nameof(StatusType.Approved)))
           ? eventRequestDto.Status
           : nameof(StatusType.Pending);
            eventRequest.AppliedSROTickets = eventRequestDto.AppliedSROTicket;
            eventRequest.ApprovedSROTickets = eventRequestDto.ApprovedSROTickets;

            eventRequest.ApproveRejectDate = (eventRequest.ApproveRejectDate != null) ? eventRequest.ApproveRejectDate : (_currentUserService.IsAdmin && eventRequestDto.Status != nameof(StatusType.Pending))? DateTimeOffset.UtcNow : null;

            _unitOfWork.GenericRepository<EventRequest>().Update(eventRequest);
            await _unitOfWork.SaveAsync();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private async Task<EventRequest> EventRequestMapping(EventRequestDto eventRequestDto)
    {
        return new EventRequest
        {
            EventId = Convert.ToInt32(eventRequestDto.EventId),
            RequestId = eventRequestDto.RequestId,
            AppliedParkingPasses = eventRequestDto.AppliedParkingPasses!,
            ExecutiveNotes = eventRequestDto.ExecutiveNotes,
            AppliedTickets = eventRequestDto.AppliedTickets!,
            ApprovedTickets = _currentUserService.IsAdmin ? eventRequestDto.ApprovedTickets : 0,
            SROTickets = eventRequestDto.SROTickets,
            CateringAndDrinks = eventRequestDto.CateringAndDrinks,
            Status = _currentUserService.IsAdmin ? eventRequestDto.Status : nameof(StatusType.Pending),
            CreatedBy = !(_currentUserService.IsAdmin) ? _currentUserService.UserId : eventRequestDto.CreatedBy,
            LastModifiedBy = _currentUserService.UserId,
            ApprovedParkingPasses = _currentUserService.IsAdmin ? eventRequestDto.ApprovedParkingPasses : 0,
            AdminNotes = _currentUserService.IsAdmin ? eventRequestDto.AdminNotes : string.Empty,
            CreatedOn = DateTimeOffset.UtcNow!,
            PackageSelected = eventRequestDto.SelectedPackage,
            AppliedSROTickets = eventRequestDto.AppliedSROTicket,
            ApprovedSROTickets = _currentUserService.IsAdmin ? eventRequestDto.ApprovedSROTickets : 0
        };
    }

}
