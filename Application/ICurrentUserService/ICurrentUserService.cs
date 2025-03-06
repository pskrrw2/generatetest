using Domain.Identity;

namespace Application.ICurrentUserService;

public interface ICurrentUserService
{
    string UserId { get; }
    bool IsAdmin { get; }
    Task<ApplicationUser> GetApplicationUserAsync(string ExcutiveId);
}
