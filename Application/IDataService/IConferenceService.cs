using Application.Dto;
using Domain.Entities;

namespace Application.IDataService;

public interface IConferenceService
{
    Task<IEnumerable<Conference>> GetAllAsync();
    Task<IEnumerable<Conference>> GetByUserIdAsync(string userId);
    Task<Conference> GetById(object id);
    Task<bool> AddAsync(ConferenceDto dto);
    Task<bool> Update(ConferenceDto dto);
    Task DeleteById(Conference dto);
}
