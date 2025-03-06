using Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Areas.Identity.Pages.Account;

public class LogoutModel(SignInManager<ApplicationUser> signInManager,
                         ILogger<LogoutModel> logger) : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly ILogger<LogoutModel> _logger = logger;

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out successfully.");

        if (!string.IsNullOrEmpty(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToPage("/account/login");

    }
}