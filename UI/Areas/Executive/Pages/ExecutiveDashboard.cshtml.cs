using Application.BlobStorage;
using Application.ICurrentUserService;
using Application.IDataService;
using Domain.Common.Const;
using Domain.Common.Enum;
using Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static UI.Areas.Executive.Pages.ExploreEventsModel;
using static UI.Areas.Executive.Pages.MyRequestsModel;

namespace UI.Areas.Executive.Pages;

[Authorize(Roles = nameof(RoleType.Executive))]
public class ExecutiveDashboardModel(
                                    IBlobStorageService blobService,
                                    IEventRequestService eventRequestService,
                                    IMatchEventService matchEventService,
                                    ICurrentUserService currentUserService,
                                    IAttendeeService attendeeService,
                                    IConferenceService conferenceService) : PageModel
{
    private readonly IBlobStorageService _blobService = blobService;
    private readonly IEventRequestService _eventRequestService = eventRequestService;
    private readonly IMatchEventService _matchEventService = matchEventService;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IAttendeeService _attendeeService = attendeeService;
    private readonly IConferenceService _conferenceService = conferenceService;

    [BindProperty]
    public List<MyRequestsModels>? RequestsModels { get; set; }

    [BindProperty]
    public int TotalRequest { get; set; }

    [BindProperty]
    public int ApproveRequest { get; set; }

    [BindProperty]
    public int PendingRequest { get; set; }

    [BindProperty]
    public int RejectRequest { get; set; }

    [BindProperty]
    public int TotalConferenceRequest { get; set; }

    [BindProperty]
    public int ApproveConFerenceRequest { get; set; }

    [BindProperty]
    public int PendingConferenceRequest { get; set; }

    [BindProperty]
    public int RejectConferenceRequest { get; set; }

    [BindProperty]
    public IEnumerable<MatchEventModel>? MatchEvent { get; set; }
    public async Task OnGet()
    {
        var eventRequests = await _eventRequestService.GetByUserIdAsync(_currentUserService.UserId);
        var conferneceRequests = await _conferenceService.GetByUserIdAsync(_currentUserService.UserId);
        var eventMatch = await _matchEventService.GetAllMatchEventAsync();
        var currentDate = DateTimeOffset.UtcNow;
        var matchEventIsactive = eventMatch.Where(e => e.IsActive && e.EventDate > currentDate).OrderBy(x => x.EventDate);

        var attendees = await _attendeeService.GetAllAsync();


        TotalRequest = eventRequests.Count();
        ApproveRequest = eventRequests.Count(x => x.Status is nameof(StatusType.Approved));
        PendingRequest = eventRequests.Count(x => x.Status is nameof(StatusType.Pending));
        RejectRequest = eventRequests.Count(x => x.Status is nameof(StatusType.Rejected));

        TotalConferenceRequest = conferneceRequests.Count();
        ApproveConFerenceRequest = conferneceRequests.Count(x => x.Status is nameof(StatusType.Approved));
        PendingConferenceRequest = conferneceRequests.Count(x => x.Status is nameof(StatusType.Pending));
        RejectConferenceRequest = conferneceRequests.Count(x => x.Status is nameof(StatusType.Rejected));


        var matchEventModels = new List<MatchEventModel>();
        foreach (var matchEvent in matchEventIsactive)
        {
            var matchEventModel = new MatchEventModel
            {
                EventId = matchEvent.EventId,
                EventName = matchEvent.EventName,
                EventVenue = matchEvent.EventVenue,
                EventThumbnail = await _blobService.DisplayImage(Path.GetFileName(matchEvent.EventThumbnail) ?? string.Empty),
                EventSession = matchEvent.EventSession,
                EventDate = matchEvent.EventDate,
                EventTime = matchEvent.EventTime,
                EventTotalTickets = await CalculateTotalTicket(matchEvent.EventTotalTickets, matchEvent.EventId)
            };

            matchEventModels.Add(matchEventModel);
        }

        MatchEvent = matchEventModels.Take(3).ToList();

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
                    AppliedSROTickets = request.AppliedSROTickets ?? 0,
                })
            .OrderByDescending(x => x.CreatedDate)
            .Take(3)
            .ToList();
    }

    public async Task<int> CalculateTotalTicket(int? TotalTickets, int eventId)
    {
        var eventrequest = await _eventRequestService.GetByEventIdAsync(eventId);
        var total = 0;
        if (eventrequest.Count() > 0)
        {
            total = eventrequest.Sum(x => x.ApprovedTickets);
        }
        var totalAvailableTickets = TotalTickets ?? 0;
        var restTotal = Math.Max(0, totalAvailableTickets - total);
        return restTotal;
    }

}
