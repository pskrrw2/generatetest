using Application.BlobStorage;
using Application.Dto;
using Application.ICurrentUserService;
using Application.IDataService;
using Application.Service;
using Domain.Common.Const;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
namespace Infrastructure.DataService;

public class MatchEventService(IBlobStorageService blobService,
                               IUnitOfWork unitOfWork,
                               ICurrentUserService currentUserService) : IMatchEventService
{
    private readonly IBlobStorageService _blobService = blobService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    public async Task<bool> AddMatchEventAsync(MatchEventDto matchEventDto)
    {
        try
        {
            var fileName = string.Empty;
            if (matchEventDto.File is not null && matchEventDto.EventThumbnail is not null)
            {
                fileName = await UploadImageAsync(matchEventDto.File!);
            }

            var matchEvent = MatchEventMapping(matchEventDto, fileName);
            if (matchEvent.Result is not null)
            {
                await _unitOfWork.GenericRepository<MatchEvent>().AddAsync(matchEvent.Result);
                await _unitOfWork.SaveAsync();
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            // Log the exception as needed
            return false;
        }
    }


    public async Task DeleteMatchEventById(MatchEvent matchEvent)
    {
        _unitOfWork.GenericRepository<MatchEvent>().Delete(matchEvent);
        await _unitOfWork.SaveAsync();
    }

    public async Task<IEnumerable<MatchEvent>> GetAllMatchEventAsync()
    {
        return await _unitOfWork.GenericRepository<MatchEvent>().GetAllAsync();
    }

    public async Task<IEnumerable<MatchEvent>> GetAllActiveMatchEventAsync()
    {
        var events = await _unitOfWork.GenericRepository<MatchEvent>().GetAllAsync();
        var activeEvents = events.Where(x => x.IsActive);
        return activeEvents;
    }

    public async Task<MatchEvent> GetMatchEventById(object id)
    {
        return await _unitOfWork.GenericRepository<MatchEvent>().GetByIdAsync(id);
    }

    public async Task<bool> UpdateMatchEvent(MatchEventDto matchEventDto)
    {
        try
        {
            var matchEvent = await GetMatchEventById(matchEventDto.EventId);
            string fileName = string.Empty;
            if (matchEventDto.File is not null && matchEventDto.EventThumbnail is not null)
            {
                //await _blobService.DeleteAsync(matchEvent.EventThumbnail);
                fileName = await UploadImageAsync(matchEventDto.File!);
            }
            else
            {
                fileName = matchEventDto.EventThumbnail!;
            }

            if (matchEvent != null)
            {
                matchEvent.EventName = matchEventDto.EventName;
                matchEvent.EventVenue = matchEventDto.EventVenue;
                matchEvent.IsActive = matchEventDto.IsActive;
                matchEvent.EventThumbnail = fileName;
                matchEvent.EventSession = matchEventDto.EventSession;
                matchEvent.EventDate = Constants.ConvertToUtc(matchEventDto.EventDate);
                matchEvent.EventTime = matchEventDto.EventTime;
                matchEvent.EventType = matchEventDto.EventType;
                matchEvent.EventTotalTickets = matchEventDto.EventTotalTickets;
                matchEvent.LastModifiedBy = _currentUserService.UserId;
                matchEvent.LastModifiedOn = DateTimeOffset.UtcNow;
                matchEvent.EventTotalParking = matchEventDto.EventTotalParking;
                matchEvent.EventTotalSROTIckets = matchEventDto.EventTotalSROTIckets;
                matchEvent.EventSROPerTicketPrice = matchEventDto.EventSROPerTicketPrice;

                _unitOfWork.GenericRepository<MatchEvent>().Update(matchEvent);
                await _unitOfWork.SaveAsync();
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating match event: {ex.Message}");
            return false;
        }
    }

    private async Task<MatchEvent> MatchEventMapping(MatchEventDto matchEventDto, string imageName)
    {
        return new MatchEvent
        {
            EventName = matchEventDto.EventName,
            EventVenue = matchEventDto.EventVenue,
            IsActive = matchEventDto.IsActive,
            EventThumbnail = imageName,
            EventSession = matchEventDto.EventSession,
            CreatedBy = _currentUserService.UserId,
            EventDate = Constants.ConvertToUtc(matchEventDto.EventDate),
            EventTime = matchEventDto.EventTime,
            EventType = matchEventDto.EventType,
            EventTotalTickets = matchEventDto.EventTotalTickets,
            CreatedOn = DateTimeOffset.UtcNow!,
            EventTotalParking = matchEventDto.EventTotalParking,
            EventTotalSROTIckets = matchEventDto.EventTotalSROTIckets,
            EventSROPerTicketPrice = matchEventDto.EventSROPerTicketPrice
        };
    }

    public async Task<string> UploadImageAsync(IFormFile request)
    {
        var bytes = GetByteArrayFromFile(request);
        string fileName = $"{Guid.NewGuid():N}{Path.GetExtension(request.FileName)}";
        string url = await _blobService.UploadAsync(bytes, fileName);

        return url;
    }

    private byte[] GetByteArrayFromFile(IFormFile? file)
    {
        using (var target = new MemoryStream())
        {
            file.CopyTo(target);
            return target.ToArray();
        }
    }
}
