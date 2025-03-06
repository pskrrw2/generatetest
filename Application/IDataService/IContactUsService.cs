using Application.Dto;
using Domain.Entities;

namespace Application.IDataService
{
    public interface IContactUsService
    {
        Task<IEnumerable<ContactUs>> GetAllAsync();
        Task<IEnumerable<ContactUs>> GetByUserIdAsync(string userId);
        Task<bool> AddAsync(ContactUs contactUsDto);
        Task DeleteById(ContactUs contactUs);
    }
}