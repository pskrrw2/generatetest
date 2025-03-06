using Application.ICurrentUserService;
using Application.IDataService;
using Domain.Common.Enum;
using Domain.Common.Enums;
using Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static UI.Areas.Admin.Pages.ManageRequestsModel;
using static UI.Areas.Executive.Pages.CreateRequestModel;

namespace UI.Areas.Admin.Pages;

[Authorize(Roles = nameof(RoleType.Admin))]
public class DashboardAdminModel(IEventRequestService eventRequestService,
   IMatchEventService matchEventService,
   UserManager<ApplicationUser> userManager,
   IAttendeeService attendeeService,
   ICurrentUserService currentUserService) : PageModel
{
    private readonly IEventRequestService _eventRequestService = eventRequestService;
    private readonly IMatchEventService _matchEventService = matchEventService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IAttendeeService _attendeeService = attendeeService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    [BindProperty]
    public int ExecutiveTotal { get; set; }

    [BindProperty]
    public int EventTotal { get; set; }

    [BindProperty]
    public int ApproveRequest { get; set; }

    [BindProperty]
    public int PendingRequest { get; set; }

    [BindProperty]
    public int RejectRequest { get; set; }

    [BindProperty]
    public List<MyRequestsModels>? RequestsModels { get; set; }
    public async Task OnGetAsync()
    {
        // Fetch event requests and match events concurrently
        var eventRequests = await _eventRequestService.GetAllAsync();
        var eventMatches = await _matchEventService.GetAllMatchEventAsync();
        var users = await GetUsersWithRoleAsync(nameof(RoleType.Executive));
        var attendees = await _attendeeService.GetAllAsync();

        // Count totals
        ApproveRequest = eventRequests.Count(x => x.Status == nameof(StatusType.Approved));
        PendingRequest = eventRequests.Count(x => x.Status == nameof(StatusType.Pending));
        RejectRequest = eventRequests.Count(x => x.Status == nameof(StatusType.Rejected));
        EventTotal = eventMatches.Count();
        ExecutiveTotal = users.Count();

        // Combine and select only the needed data
        RequestsModels = eventRequests
                        .Join(eventMatches,
                              request => request.EventId,
                              eventMatch => eventMatch.EventId,
                              (request, eventMatch) => new { request, eventMatch })
                        .Join(users,
                              combined => combined.request.CreatedBy,
                              user => user.Id,
                              (combined, user) => new { combined.request, combined.eventMatch, user })
                        .OrderByDescending(x => x.request.CreatedOn)
                        .Take(3)
                        .Select(x => new MyRequestsModels
                        {
                            RequestId = x.request.RequestId,
                            EventId = x.eventMatch.EventId,
                            EventName = x.eventMatch.EventName,
                            EventVenue = x.eventMatch.EventVenue,
                            EventDate = x.eventMatch.EventDate,
                            AppliedTickets = x.request.AppliedTickets,
                            AppliedParkingPasses = x.request.AppliedParkingPasses,
                            Status = x.request.Status,
                            SROTickets = x.request.SROTickets,
                            CreatedDate = x.request.CreatedOn,
                            ExecutiveName = $"{x.user.FirstName} {x.user.LastName}",
                            ApproveTicket = x.request.ApprovedTickets,
                            ApproveParking = x.request.ApprovedParkingPasses,
                            Attendees = attendees.Where(y => y.RequestId == x.request.RequestId).ToList(),
                            PackageSelected = x.request.PackageSelected,
                            AddOnSelected = x.request.AddonsSelected,
                            IsAdmin = _currentUserService.IsAdmin ? true : false,
                            ApproveSROTicket = x.request.ApprovedSROTickets ?? 0,
                            AppliedSROTickets = x.request.AppliedSROTickets ?? 0
                        })
                        .ToList();
    }

    public async Task<IActionResult> OnGetAdminDashboardPartialAsync(int requestId, int eventId, string action)
    {
        var eventMatch = await _matchEventService.GetMatchEventById(eventId);
        var eventRequests = await _eventRequestService.GetAllAsync();
        var eventRequest = eventRequests.FirstOrDefault(x => x.RequestId == requestId);

        var user = await _userManager.FindByIdAsync(eventRequest?.CreatedBy!);

        // Calculate total requests in a single iteration
        var (totalPassRequest, totalParkingRequest, totalSROPasses) = eventRequests
            .Where(x => x.EventId == eventId && x.Status == nameof(StatusType.Approved))
            .Aggregate((Passes: 0, Parking: 0, SROPasses: 0), (acc, x) => (acc.Passes + x.ApprovedTickets, acc.Parking + x.ApprovedParkingPasses, acc.SROPasses + x.ApprovedSROTickets ?? 0));


        var availableSeat = Math.Max(0, eventMatch?.EventTotalTickets ?? 0 - totalPassRequest);
        var availableParking = Math.Max(0, eventMatch?.EventTotalParking ?? 0 - totalParkingRequest);
        var availableSROSeat = Math.Max(0, eventMatch?.EventTotalSROTIckets ?? 0 - totalSROPasses);


        var requestModel = new RequestModel
        {
            EventName = eventMatch?.EventName,
            ExecutiveName = $"{user?.FirstName} {user?.LastName}",
            CreatedDate = eventRequest?.CreatedOn,
            AppliedTickets = eventRequest.AppliedTickets,
            AppliedParkingPasses = eventRequest.AppliedParkingPasses,
            RequestId = eventRequest.RequestId,
            AdminNotes = eventRequest.AdminNotes,
            ApprovedTickets = eventRequest.ApprovedTickets,
            SROTickets = (bool)eventRequest.SROTickets! ? SROType.Yes : SROType.No,
            CateringAndDrinks = eventRequest.CateringAndDrinks.HasValue,
            EventId = eventRequest.EventId,
            ExecutiveId = eventRequest.CreatedBy,
            ExecutiveNotes = eventRequest.ExecutiveNotes,
            ApprovedParkingPasses = eventRequest.ApprovedParkingPasses,
            AvailableSeat = availableSeat,
            AvailableParking = availableParking,
            AvailableSROSeat = availableSROSeat,
            ApprovedSROTickets = eventRequest.ApprovedSROTickets ?? 0,
            AppliedSROTicket = eventRequest.AppliedSROTickets ?? 0,
            PackageSelected = eventRequest.PackageSelected,
            AddOnSelected = eventRequest.AddonsSelected,
        };

        return action switch
        {
            "Approved" => Partial("~/Areas/Shared/Modal/EventRequest/_ApproveEventRequest.cshtml", requestModel),
            "Reject" => Partial("~/Areas/Shared/Modal/EventRequest/_RejectEventRequest.cshtml", requestModel),
            _ => Page()
        };
    }

    private async Task<IEnumerable<ApplicationUser>> GetUsersWithRoleAsync(string roleName) =>
    await _userManager.GetUsersInRoleAsync(roleName);

}
