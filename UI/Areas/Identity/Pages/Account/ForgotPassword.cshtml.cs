using Application.Mailing;
using AspNetCoreHero.ToastNotification.Abstractions;
using Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UI.Areas.Identity.Pages.Account;

public class ForgotPasswordModel(UserManager<ApplicationUser> userManager,
                                 IMailManagerService mailManagerService,
                                 INotyfService toastNotification) : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IMailManagerService _mailManagerService = mailManagerService;
    private readonly INotyfService _toastNotification= toastNotification;


    [BindProperty]
    public InputModel? Input { get; set; }
    public class InputModel
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user is null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Invalid Account.");
                return Page();
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            const string route = "account/reset-password";
            var endpointUri = new Uri($"{GetOriginFromRequest()}/{route}");
            var resetUri = QueryHelpers.AddQueryString(endpointUri.ToString(), "code", code);

            await _mailManagerService.SendForgotPasswordEmailAsync(Input.Email, resetUri);

            _toastNotification.Success("Please check your email to reset your password.");
        }
        catch (Exception ex)
        {
            _toastNotification.Error("An error occurred while processing your request. Please try again later.");
        }

        return Page();
    }


    protected string GetOriginFromRequest()
    {
        return $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
    }

}
