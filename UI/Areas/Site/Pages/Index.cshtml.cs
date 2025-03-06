using Domain.Common.Const;
using Domain.Common.Enum;
using Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Areas.Site.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public IndexModel(ILogger<IndexModel> logger,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _logger = logger;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    [BindProperty]
    public string? UserId { get; set; }
    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        var user = await _signInManager.UserManager.GetUserAsync(User);
        if (user == null || !user.IsActive || !user.EmailConfirmed)
            return RedirectToPage(PageNames.Login, new { area = WebAreas.Identity });

        var isExecutive = await _signInManager.UserManager.IsInRoleAsync(user, nameof(RoleType.Executive));
        if (isExecutive)
            return RedirectToPage(PageNames.ExecutiveDashboard, new { area = WebAreas.Executive });

        var isAdmin = await _signInManager.UserManager.IsInRoleAsync(user, nameof(RoleType.Admin));
        if (isAdmin)
            return RedirectToPage(PageNames.DashboardAdmin, new { area = WebAreas.Administration });

        return RedirectToPage(PageNames.Login, new { area = WebAreas.Identity });
    }

}