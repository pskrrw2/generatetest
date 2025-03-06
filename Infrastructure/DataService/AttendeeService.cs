using Application.Dto;
using Application.ICurrentUserService;
using Application.IDataService;
using Application.Service;
using Domain.Entities;

namespace Infrastructure.DataService;

public class AttendeeService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : IAttendeeService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    public async Task<bool> AddAsync(AttendeeDto attendeeDto)
    {
        try
        {
            var attendee = AttendeeMapping(attendeeDto);

            await _unitOfWork.GenericRepository<Attendee>().AddAsync(attendee);
            await _unitOfWork.SaveAsync();

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task DeleteById(Attendee attendee)
    {
        _unitOfWork.GenericRepository<Attendee>().Delete(attendee);
        await _unitOfWork.SaveAsync();
    }

    public async Task<IEnumerable<Attendee>> GetAllAsync()
    {
        return await _unitOfWork.GenericRepository<Attendee>().GetAllAsync();
    }

    public async Task<IEnumerable<Attendee>> GetByUserIdAsync(string userId)
    {
        var attendees = await GetAllAsync();
        return attendees.Where(x => x.UserId == userId);
    }

    public async Task<Attendee> GetById(object id)
    {
        return await _unitOfWork.GenericRepository<Attendee>().GetByIdAsync(id);
    }

    public async Task<bool> Update(AttendeeDto attendeeDto)
    {
        try
        {
            var attendee = await GetById(attendeeDto.Id);
            if (attendee is null)
                throw new ArgumentException($"Attendee with ID {attendeeDto.Id} not found.");

            attendee.RequestId = attendeeDto.RequestId ?? 0;
            attendee.Name = attendeeDto.Name!;
            attendee.EmailId = attendeeDto.EmailId;
            attendee.MobileNumber = attendeeDto.MobileNumber!;
            attendee.TicketsAssigned = attendeeDto.TicketsAssigned;
            attendee.NoteToAttendee = attendeeDto.NoteToAttendee;
            attendee.LastModifiedBy = _currentUserService.UserId;
            attendee.LastModifiedOn = DateTimeOffset.UtcNow;
            attendee.CreatedBy = !_currentUserService.IsAdmin
                                 ? _currentUserService.UserId
                                 : attendeeDto.UserId;
            attendee.SeatType = attendeeDto.AttendeeSeatType.ToString();
            attendee.UserId = attendeeDto.UserId;
            attendee.ConferenceId = attendeeDto.ConferenceId ?? null;

            _unitOfWork.GenericRepository<Attendee>().Update(attendee);
            await _unitOfWork.SaveAsync();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private Attendee AttendeeMapping(AttendeeDto attendeeDto)
    {
        return new Attendee
        {
            RequestId = attendeeDto.RequestId ?? 0,
            Name = attendeeDto.Name!,
            EmailId = attendeeDto.EmailId,
            MobileNumber = attendeeDto.MobileNumber!,
            TicketsAssigned = attendeeDto.TicketsAssigned,
            NoteToAttendee = attendeeDto.NoteToAttendee,
            CreatedOn = DateTimeOffset.UtcNow!,
            SeatType = attendeeDto.AttendeeSeatType.ToString(),
            UserId = attendeeDto.UserId,
            ConferenceId = attendeeDto.ConferenceId ?? null,
        };
    }

    public async Task<IEnumerable<Attendee>> GetByRequestIdAsync(int requestId)
    {
        var attendees = await _unitOfWork.GenericRepository<Attendee>().GetAllAsync();
        return attendees.Where(x => x.RequestId == requestId);
    }

    public async Task<IEnumerable<Attendee>> GetByConferenceIdAsync(int conferenceId)
    {
        var attendees = await _unitOfWork.GenericRepository<Attendee>().GetAllAsync();
        return attendees.Where(x => x.ConferenceId == conferenceId);
    }

    public async Task<IEnumerable<Attendee>> GetByUserIdAndRequestIdAsync(string userId, int requestId)
    {
        var attendees = await _unitOfWork.GenericRepository<Attendee>().GetAllAsync();
        return attendees.Where(x => x.RequestId == requestId && x.UserId == userId);
    }
}
