using Application.Dto;
using Application.ICurrentUserService;
using Application.IDataService;
using Application.Mailing;
using Application.Service;
using Domain.Entities;
using Infrastructure.CurrentUserService;
using Infrastructure.Features;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataService
{
    public class ContactUsService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : IContactUsService
    {

        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        public async Task<bool> AddAsync(ContactUs contact)
        {
            var contactUs = ContactUsMapping(contact);
            if (contactUs is not null)
            {
                await _unitOfWork.GenericRepository<ContactUs>().AddAsync(contactUs);
                await _unitOfWork.SaveAsync();
                return true;
            }
            return false;
        }

        public async Task DeleteById(ContactUs contactUs)
        {
            _unitOfWork.GenericRepository<ContactUs>().Delete(contactUs);
            await _unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<ContactUs>> GetAllAsync()
        {
            return await _unitOfWork.GenericRepository<ContactUs>().GetAllAsync();
        }

        public async Task<IEnumerable<ContactUs>> GetByUserIdAsync(string userId)
        {
            var results = await _unitOfWork.GenericRepository<ContactUs>().GetAllAsync();
            return results.Where(x => x.CreatedBy == userId);
        }

        private ContactUs ContactUsMapping(ContactUs contactUs) => new ContactUs
        {
            Message = contactUs.Message,
            CreatedBy = _currentUserService.UserId,
            CreatedOn = DateTimeOffset.UtcNow!
        };
    }
}
