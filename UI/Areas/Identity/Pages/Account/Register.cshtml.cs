using Application.Mailing;
using AspNetCoreHero.ToastNotification.Abstractions;
using Domain.Common.Enum;
using Domain.Common.Mailing;
using Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace UI.Areas.Identity.Pages.Account;

[Authorize(Roles = "Admin")]
public class RegisterModel : PageModel
{

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IMailManagerService _mailManagerService;
    private readonly INotyfService _toastNotification;

    public RegisterModel(UserManager<ApplicationUser> userManager,
                           ILogger<RegisterModel> logger,
                           IMailManagerService mailManagerService,
                           INotyfService toastNotification)
    {
        _userManager = userManager;
        _logger = logger;
        _mailManagerService = mailManagerService;
        _toastNotification = toastNotification;
    }


    [BindProperty]
    public InputModel? Input { get; set; }
    public string? ReturnUrl { get; set; }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        if (!ModelState.IsValid)
            return Page();

        var user = new ApplicationUser
        {
            FirstName = Input!.FirstName,
            LastName = Input!.LastName,
            Email = Input!.Email,
            UserName = Input.Email,
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                var errorMessage = error.Code switch
                {
                    "DuplicateUserName" => "A user with this email address already exists.",
                    _ => error.Description
                };

                ModelState.AddModelError(string.Empty, errorMessage);
                break;
            }

            return Page();
        }

        _logger.LogInformation("User created a new account with password.");

        var roleName = string.Empty;

        if (Input.IsAdmin)
            roleName = nameof(RoleType.Admin);
        else
            roleName = nameof(RoleType.Executive);

        await _userManager.AddToRoleAsync(user, roleName);

        await _mailManagerService.SendNewRegisterAccountAsync(
           new SendEmailConfirmationEmailModel(user.Email, string.Empty, string.Empty));

        _toastNotification.Success($"{Input.Email} invited successfully");
        return RedirectToPage();
    }

    public class InputModel
    {
        [Display(Name = "First Name")]
        public required string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Please enter e-mail ID")]
        [EmailAddress]
        [Display(Name = "Email")]
        public required string Email { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
    }

}
