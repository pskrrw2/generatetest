using Application.Mailing;
using Domain.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace UI.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LoginModel> _logger;
    private readonly IMailManagerService _mailManagerService;

    public LoginModel(SignInManager<ApplicationUser> signInManager,
                      ILogger<LoginModel> logger,
                      UserManager<ApplicationUser> userManager,
                      IMailManagerService mailManagerService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _mailManagerService = mailManagerService;
    }

    [BindProperty]
    public InputModel? Input { get; set; }

    [BindProperty]
    public bool IsExistEmail { get; set; }
    public IList<AuthenticationScheme> ExternalLogins { get; set; }
    public string ReturnUrl { get; set; }

    [TempData]
    public string ErrorMessage { get; set; }
    public class InputModel
    {

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "OTP Number")]
        public string? OtpNumber { get; set; }

        //[Required]
        //[DataType(DataType.Password)]
        //public required string Password { get; set; }

        //[Display(Name = "Remember me?")]
        //public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
            ModelState.AddModelError(string.Empty, ErrorMessage);

        returnUrl ??= Url.Content("~/");
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostOtpSendAsync()
    {
        if (!ModelState.IsValid)
            return new JsonResult(new { errorMessage = "The Email field is not a valid e-mail address." });

        Random random = new Random();
        var otpCode = random.Next(100000, 999999).ToString();
        var expirationTime = DateTimeOffset.UtcNow.AddMinutes(5);

        var user = await _userManager.FindByNameAsync(Input?.Email!);
        if (user == null)
        {
            return new JsonResult(new { errorMessage = "This email is not registered. Please contact Administrator." });
          //  ModelState.AddModelError("Input.Email", "This email is not registered. Please contact Administrator.");
          //  return new JsonResult("fail");
        }

        bool IsExistEmail = user.EmailConfirmed && user.IsActive;
        if (!IsExistEmail)
        {
            return new JsonResult(new { errorMessage = "Your account is currently inactive or not confirmed. Please contact Administrator." });
           // ModelState.AddModelError("Input.Email", "Your account is currently inactive or not confirmed. Please contact Administrator.");
          //  return new JsonResult("fail");
        }

        user.OtpNumber = otpCode;
        user.OtpExpire = expirationTime;
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            await _mailManagerService.SendOtpMail(Input?.Email!, otpCode);
            return new JsonResult(new { success = "success" });
            //return new JsonResult("success");
        }
        else
        {
            return new JsonResult(new { fail = "fail" });
        }
    }

    public async Task<IActionResult> OnPostVerifyOtpAsync()
    {
        if (Input?.OtpNumber == null)
            return new JsonResult(new { errorMessage = "Please enter OTP." });

        var user = await _userManager.FindByNameAsync(Input.Email!);
        if (user == null)
            return new JsonResult(new { errorMessage = "This email is not registered. Please contact Administrator." });

        if (user.OtpExpire < DateTimeOffset.UtcNow)
            return new JsonResult(new { errorMessage = "OTP has been expired." });

        if (user.OtpNumber == Input.OtpNumber)
        {
            await _signInManager.SignInAsync(user, false);
            _logger.LogInformation("User logged in.");
            return new JsonResult(new { redirectUrl = Url.Content("~/") });
        }

        return new JsonResult(new { errorMessage = "Please enter valid OTP." });
    }
}