
using Application.Dto;
using Domain.Entities;

namespace Application.IDataService;
public interface IMatchEventService
{
    Task<IEnumerable<MatchEvent>> GetAllMatchEventAsync();
    Task<IEnumerable<MatchEvent>> GetAllActiveMatchEventAsync();
    Task<MatchEvent> GetMatchEventById(object id);
    Task<bool> AddMatchEventAsync(MatchEventDto dto);
    Task<bool> UpdateMatchEvent(MatchEventDto dto);
    Task DeleteMatchEventById(MatchEvent dto);
}
