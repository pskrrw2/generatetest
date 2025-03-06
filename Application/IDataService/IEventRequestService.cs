using Application.Dto;
using Domain.Entities;

namespace Application.IDataService;

public interface IEventRequestService
{
    Task<IEnumerable<EventRequest>> GetAllAsync();
    Task<IEnumerable<EventRequest>> GetByUserIdAsync(string userId);
    Task<IEnumerable<EventRequest>> GetByEventIdAsync(int eventId);
    Task<EventRequest> GetById(object id);
    Task<bool> AddAsync(EventRequestDto dto);
    Task<bool> Update(EventRequestDto dto);
    Task DeleteById(EventRequest dto);
}
