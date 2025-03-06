using Application.Dto;
using Application.ICurrentUserService;
using Application.IDataService;
using Application.Service;
using Domain.Entities;

namespace Infrastructure.DataService;

public class AddOnMasterService(IUnitOfWork unitOfWork) : IAddOnMasterService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task<bool> AddAsync(AddOnMasterDto addOnMasterDto)
    {
        try
        {
            var addOnMaster = AddOnMasterMapping(addOnMasterDto);

            await _unitOfWork.GenericRepository<AddOnMaster>().AddAsync(addOnMaster);
            await _unitOfWork.SaveAsync();

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task DeleteById(AddOnMaster addOnMaster)
    {
        _unitOfWork.GenericRepository<AddOnMaster>().Delete(addOnMaster);
        await _unitOfWork.SaveAsync();
    }

    public async Task<IEnumerable<AddOnMaster>> GetAllAsync()
    {
        return await _unitOfWork.GenericRepository<AddOnMaster>().GetAllAsync();
    }
    public async Task<AddOnMaster> GetById(object id)
    {
        return await _unitOfWork.GenericRepository<AddOnMaster>().GetByIdAsync(id);
    }

    public async Task<bool> Update(AddOnMasterDto addOnMasterDto)
    {
        try
        {
            var addOnMaster = await GetById(addOnMasterDto.Id);
            if (addOnMaster is null)
                throw new ArgumentException($"AddOnMaster with ID {addOnMasterDto.Id} not found.");

            _unitOfWork.GenericRepository<AddOnMaster>().Update(addOnMaster);
            await _unitOfWork.SaveAsync();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private AddOnMaster AddOnMasterMapping(AddOnMasterDto addOnMasterDto)
    {
        return new AddOnMaster
        {
            Id = addOnMasterDto.Id,
        };
    }
}
