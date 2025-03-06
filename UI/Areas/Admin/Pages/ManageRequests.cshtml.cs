using Application.ICurrentUserService;
using Application.IDataService;
using Domain.Common.Enum;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Areas.Admin.Pages;

[Authorize(Roles = nameof(RoleType.Admin))]
public class ManageRequestsModel(IEventRequestService eventRequestService,
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
    public int TotalRequest { get; set; }

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
        var eventRequests = await _eventRequestService.GetAllAsync();
        var eventMatch = await _matchEventService.GetAllMatchEventAsync();
        var users = await GetUsersWithRoleAsync(nameof(RoleType.Executive));
        var attendees = await _attendeeService.GetAllAsync();

        TotalRequest = eventRequests.Count();
        ApproveRequest = eventRequests.Count(x => x.Status == nameof(StatusType.Approved));
        PendingRequest = eventRequests.Count(x => x.Status == nameof(StatusType.Pending));
        RejectRequest = eventRequests.Count(x => x.Status == nameof(StatusType.Rejected));

        RequestsModels = (from request in eventRequests
                          join eventMath in eventMatch on request.EventId equals eventMath.EventId
                          join user in users on request.CreatedBy equals user.Id
                          select new MyRequestsModels
                          {
                              RequestId = request.RequestId,
                              EventId = eventMath.EventId,
                              EventName = eventMath.EventName,
                              EventVenue = eventMath.EventVenue,
                              EventDate = eventMath.EventDate,
                              AppliedTickets = request.AppliedTickets,
                              AppliedParkingPasses = request.AppliedParkingPasses,
                              Status = request.Status,
                              SROTickets = request.SROTickets,
                              CreatedDate = request.CreatedOn,
                              ExecutiveName = $"{user.FirstName} {user.LastName}",
                              ApproveTicket = request.ApprovedTickets,
                              ApproveParking = request.ApprovedParkingPasses,
                              Attendees = attendees.Where(x => x.RequestId == request.RequestId).ToList(),
                              PackageSelected = request.PackageSelected,
                              AddOnSelected = request.AddonsSelected,
                              IsAdmin = _currentUserService.IsAdmin ? true : false,
                              ApproveSROTicket = request.ApprovedSROTickets ?? 0,
                              AppliedSROTickets = request.AppliedSROTickets ?? 0
                          })
                     .OrderByDescending(x => x.CreatedDate)
                     .ToList();

    }

    private async Task<IEnumerable<ApplicationUser>> GetUsersWithRoleAsync(string roleName) =>
    await _userManager.GetUsersInRoleAsync(roleName);

    public class MyRequestsModels
    {
        public int EventId { get; set; }
        public int RequestId { get; set; }
        public string? EventName { get; set; }
        public string? EventVenue { get; set; }
        public string? BookTypeId { get; set; }
        public DateTimeOffset? EventDate { get; set; }
        public int AppliedTickets { get; set; }
        public int AppliedParkingPasses { get; set; }
        public int AppliedSROTickets { get; set; }
        public string? Status { get; set; }
        public bool? SROTickets { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? ExecutiveName { get; set; }  
        public int ApproveTicket { get; set; }
        public int ApproveParking { get; set; }
        public int ApproveSROTicket { get; set; }
        public string? AdminMessage { get; set; }
        public bool? CateringAndDrinks { get; set; }
        public string? ExecutiveId { get; set; }
        public string? ExecutiveDescription { get; set; }
        public IEnumerable<Attendee>? Attendees { get; set; }
        public string? PackageSelected { get; set; }
        public string? AddOnSelected { get; set; }
        public bool IsAdmin { get; set; }
    }
}
