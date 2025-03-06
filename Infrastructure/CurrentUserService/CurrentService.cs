using Application.ICurrentUserService;
using Domain.Common.Enum;
using Domain.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Infrastructure.CurrentUserService;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor,
    UserManager<ApplicationUser> userManager) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public string UserId { get; } = httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public bool IsAdmin { get; } = httpContextAccessor?.HttpContext?.User?.IsInRole(nameof(RoleType.Admin)) ?? false;
    public async Task<ApplicationUser> GetApplicationUserAsync(string userId) => await _userManager.FindByIdAsync(userId);
}
