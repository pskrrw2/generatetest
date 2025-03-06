using Application.Dto;
using Domain.Entities;

namespace Application.IDataService;

public interface IAddOnMasterService
{
    Task<IEnumerable<AddOnMaster>> GetAllAsync();
    Task<AddOnMaster> GetById(object id);
    Task<bool> AddAsync(AddOnMasterDto dto);
    Task<bool> Update(AddOnMasterDto dto);
    Task DeleteById(AddOnMaster dto);
}
