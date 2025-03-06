using Domain.Common.Enum;
using Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Areas.Admin.Pages;

[Authorize(Roles = nameof(RoleType.Admin))]
public class ManageExecutivesModel(UserManager<ApplicationUser> userManager) : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    public IEnumerable<ApplicationUser>? Users { get; set; }

    public async Task OnGetAsync() =>
      Users = await _userManager.GetUsersInRoleAsync(nameof(RoleType.Executive));

    public async Task<IActionResult> OnPostIsActiveUserAsync(string userId, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        user.IsActive = isActive;
        var result = await _userManager.UpdateAsync(user);

        var status = result.Succeeded switch
        {
            true => "success",
            false => "fail"
        };

        return new JsonResult(status);
    }
}
