using Application.ICurrentUserService;
using Application.IDataService;
using Domain.Common.Enum;
using Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static UI.Areas.Executive.Pages.CreateConferenceRequestModel;

namespace UI.Areas.Executive.Pages;

[Authorize]
public class ConferencesModel(  IConferenceService conferenceService,
                                ICurrentUserService currentUserService,
                                UserManager<ApplicationUser> userManager,
                                IAttendeeService attendeeService) : PageModel
{
    private readonly IConferenceService _conferenceService = conferenceService;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IAttendeeService _attendeeService = attendeeService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    [BindProperty]
    public IEnumerable<ConferenceModel>? Conferences { get; set; }

    [BindProperty]
    public bool IsAdmin { get; set; }
    public async Task OnGet()
    {
        IsAdmin = _currentUserService.IsAdmin;
        var conferences = _currentUserService.IsAdmin ? await _conferenceService.GetAllAsync()
            : await _conferenceService.GetByUserIdAsync(_currentUserService.UserId);

        var attendees = await _attendeeService.GetAllAsync();

        var users = await GetUsersWithRoleAsync(nameof(RoleType.Executive));

        Conferences = from conference in conferences
                      join user in users on conference.CreatedBy equals user.Id
                      orderby conference.CreatedOn descending
                      select new ConferenceModel
                      {
                          ConferenceId = conference.ConferenceId,
                          SuiteType = conference.SuiteType!,
                          ConferenceDate = conference.ConferenceDate,
                          ConferenceNotes = conference.ConferenceNotes,
                          AdminConferenceNotes = conference.AdminConferenceNotes,
                          Status = conference.Status,
                          CreatedDate = conference.CreatedOn,
                          ExecutiveId = conference.CreatedBy,
                          ExecutiveName = $"{user.FirstName} {user.LastName}",
                          Attendees = attendees.Where(x => x.ConferenceId == conference.ConferenceId),
                          SelectedAddOnIds = ParseSelectedAddOnIds(conference.AddonsSelected),
                          PackageSelected = conference.PackageSelected
                      };
    }

    public async Task<IActionResult> OnGetAdminConferenceApproveRejectAsync(int conferenceId, string action, string executiveName)
    {
        var confernceM = await _conferenceService.GetById(conferenceId);

        var conferenceModel = new ConferenceModel
        {
            ConferenceId = confernceM.ConferenceId,
            SuiteType = confernceM.SuiteType!,
            ConferenceDate = confernceM.ConferenceDate,
            ConferenceNotes = confernceM.ConferenceNotes,
            AdminConferenceNotes = confernceM.AdminConferenceNotes,
            Status = confernceM.Status,
            CreatedDate = confernceM.CreatedOn,
            Action = action,
            ExecutiveId = confernceM.CreatedBy,
            ExecutiveName = executiveName,
            ModifiedDate = confernceM.LastModifiedOn,
            AddOnDate = confernceM.AddOnDate,
            PackageDate = confernceM.PackageDate,
            ApproveRejectDate = confernceM.ApproveRejectDate
        };

        return action switch
        {
            "Approved" => Partial("~/Areas/Shared/Modal/Conference/_ApproveConferenceRequest.cshtml", conferenceModel),
            "Reject" => Partial("~/Areas/Shared/Modal/Conference/_RejectConferenceRequest.cshtml", conferenceModel),
            _ => Page()
        };
    }

    private async Task<IEnumerable<ApplicationUser>> GetUsersWithRoleAsync(string roleName) =>
        await _userManager.GetUsersInRoleAsync(roleName);

    public static List<int>? ParseSelectedAddOnIds(string? addonsSelected) =>
      string.IsNullOrWhiteSpace(addonsSelected)
          ? null
          : addonsSelected
              .Split(',', StringSplitOptions.RemoveEmptyEntries)
              .Select(int.Parse)
              .ToList();
}
