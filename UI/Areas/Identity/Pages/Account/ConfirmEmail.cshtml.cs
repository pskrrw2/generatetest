using Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace UI.Areas.Identity.Pages.Account;

public class ConfirmEmailModel(UserManager<ApplicationUser> userManager) : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    [TempData]
    public string? ConfirmStatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string? userId, string? code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            return RedirectToPage("/");

        ConfirmStatusMessage = await ConfirmEmailAsync(userId, code, cancellationToken);

        return Page();
    }

    public async Task<string> ConfirmEmailAsync(string userId, string code, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .Where(u => u.Id == userId && !u.EmailConfirmed)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return "Email has already been confirmed.";

        var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ConfirmEmailAsync(user, decodedCode);

        return result.Succeeded
            ? $"Account Confirmed for E-Mail {user.Email}."
            : throw new InvalidOperationException($"An error occurred while confirming {user.Email}");
    }
}