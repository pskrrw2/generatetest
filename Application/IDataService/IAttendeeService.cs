using Application.Dto;
using Domain.Entities;

namespace Application.IDataService;

public interface IAttendeeService
{
    Task<IEnumerable<Attendee>> GetAllAsync();
    Task<IEnumerable<Attendee>> GetByUserIdAsync(string userId);
    Task<IEnumerable<Attendee>> GetByRequestIdAsync(int requestId);
    Task<IEnumerable<Attendee>> GetByConferenceIdAsync(int conferenceId);
    Task<Attendee> GetById(object id);
    Task<bool> AddAsync(AttendeeDto dto);
    Task<bool> Update(AttendeeDto dto);
    Task DeleteById(Attendee dto);
    Task<IEnumerable<Attendee>> GetByUserIdAndRequestIdAsync(string userId, int requestId);
}
