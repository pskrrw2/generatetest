using Application.ICurrentUserService;
using Application.IDataService;
using Application.Mailing;
using AspNetCoreHero.ToastNotification.Abstractions;
using Domain.Common.Enum;
using Domain.Entities;
using Domain.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace UI.Areas.Executive.Pages;

[Authorize(Roles = nameof(RoleType.Executive))]
public class ContactUsModel(IContactUsService contactUsService,
    INotyfService toastNotification,
    ICurrentUserService currentUserService,
    IMailManagerService mailManagerService) : PageModel
{
    private readonly IContactUsService _contactUsService = contactUsService;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly INotyfService _toastNotification = toastNotification;
    private readonly IMailManagerService _mailManagerService = mailManagerService;

    [BindProperty]
    public InputModel? Input { get; set; }
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return NotFound();

        var contactUs = new ContactUs
        {
            Message = Input.Message
        };

        var result = await _contactUsService.AddAsync(contactUs);

        if (!result)
            return Page();

        var message = result switch
        {
            true => "Your message has been successfully sent.",
            false => "There was an error sending your message. Please try again later.",
        };

        if (result)
        {
            var queryRequest = await MappingQueryMailRequest(Input.Message);
             await _mailManagerService.SendQueryEmailAsync(queryRequest);
            _toastNotification.Success(message);
        }
        else
            _toastNotification.Warning(message);

        return _currentUserService.IsAdmin
        ? RedirectToPage(PageNames.ManageRequests, new { area = "Admin" })
        : RedirectToPage(PageNames.MyRequests, new { area = "Executive" });
    }

    private async Task<MailRequestVm> MappingQueryMailRequest(string message)
    {
        var user = await _currentUserService.GetApplicationUserAsync(_currentUserService.UserId);

        return new MailRequestVm
        {
            ExecutiveName = $"{user.FirstName} {user.LastName}",
            ExecutiveEmail = user.Email,
            EventName = message,
        };
    }
    public ContactUs MappingModel(string message)
    {
        return new ContactUs
        {
            Message = message
        };
    }

    public class InputModel
    {
        [Required]
        public required string Message { get; set; }
    }
}
