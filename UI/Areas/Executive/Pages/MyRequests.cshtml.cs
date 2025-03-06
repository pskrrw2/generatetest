using Application.ICurrentUserService;
using Application.IDataService;
using Domain.Common.Enum;
using Domain.Common.Enums;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Areas.Executive.Pages;

[Authorize(Roles = nameof(RoleType.Executive))]
public class MyRequestsModel(IEventRequestService eventRequestService,
    IMatchEventService matchEventService,
    ICurrentUserService currentUserService,
    IAttendeeService attendeeService) : PageModel
{
    private readonly IEventRequestService _eventRequestService = eventRequestService;
    private readonly IMatchEventService _matchEventService = matchEventService;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IAttendeeService _attendeeService = attendeeService;

    [BindProperty]
    public int TotalRequest { get; set; }

    [BindProperty]
    public int ApproveRequest { get; set; }

    [BindProperty]
    public int PendingRequest { get; set; }

    [BindProperty]
    public int RejectRequest { get; set; }

    public List<MyRequestsModels>? RequestsModels { get; set; }

    public async Task OnGetAsync()
    {
        var eventRequests = (await _eventRequestService.GetByUserIdAsync(_currentUserService.UserId)).ToList();
        var eventMatch = (await _matchEventService.GetAllMatchEventAsync()).ToList();
        var attendees = await _attendeeService.GetAllAsync();

        TotalRequest = eventRequests.Count;
        ApproveRequest = eventRequests.Count(x => x.Status is nameof(StatusType.Approved));
        PendingRequest = eventRequests.Count(x => x.Status is nameof(StatusType.Pending));
        RejectRequest = eventRequests.Count(x => x.Status is nameof(StatusType.Rejected));

        RequestsModels = eventRequests
            .Join(eventMatch, request => request.EventId, eventMath => eventMath.EventId,
                (request, eventMath) => new MyRequestsModels
                {
                    RequestId = request.RequestId,
                    EventId = eventMath.EventId,
                    EventName = eventMath.EventName,
                    Venue = eventMath.EventVenue,
                    EventDate = eventMath.EventDate,
                    RequestPasses = request.AppliedTickets,
                    AppliedParkingPasses = request.AppliedParkingPasses,
                    Status = request.Status,
                    SROPasses = request.SROTickets,
                    CreatedDate = request.CreatedOn,
                    ModifiedDate = request.LastModifiedOn,
                    Attendees = attendees.Where(x => x.RequestId == request.RequestId),
                    ApprovedTicket = request.ApprovedTickets,
                    ApprovedParking = request.ApprovedParkingPasses,
                    PackageSelected = request.PackageSelected,
                    AddOnSelected = request.AddonsSelected,
                    ApprovedSROTicket = request.ApprovedSROTickets ?? 0,
                    AppliedSROTickets = request.AppliedSROTickets ?? 0
                })
            .OrderByDescending(x => x.CreatedDate)
            .ToList();
    }

    public class MyRequestsModels
    {
        public int EventId { get; set; }
        public int RequestId { get; set; }
        public string? EventName { get; set; }
        public string? Venue { get; set; }
        public DateTimeOffset? EventDate { get; set; }
        public int RequestPasses { get; set; }
        public int AppliedParkingPasses { get; set; }
        public int AppliedSROTickets { get; set; }
        public string? Status { get; set; }
        public bool? SROPasses { get; set; }
        public SROType? SROType { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public bool? CateringAndDrinks { get; set; }
        public string? Description { get; set; }
        public IEnumerable<MatchEvent>? MatchEvents { get; set; }
        public IEnumerable<Attendee>? Attendees { get; set; }
        public int ApprovedTicket { get; set; }
        public int ApprovedParking { get; set; }
        public int ApprovedSROTicket { get; set; }
        public string? PackageSelected { get; set; }
        public string? AddOnSelected { get; set; }

    }
}
