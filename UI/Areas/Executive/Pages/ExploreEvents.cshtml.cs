using Application.BlobStorage;
using Application.IDataService;
using Domain.Common.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Areas.Executive.Pages;

[Authorize(Roles = nameof(RoleType.Executive))]
public class ExploreEventsModel(IBlobStorageService blobService, IMatchEventService matchEventService, IEventRequestService eventRequestService) : PageModel
{
    private readonly IBlobStorageService _blobService = blobService;
    private readonly IMatchEventService _matchEventService = matchEventService;
    private readonly IEventRequestService _eventRequestService = eventRequestService;

    [BindProperty]
    public IEnumerable<MatchEventModel>? MatchEvent { get; set; }

    public async Task OnGet()
    {
        var matchEvents = await _matchEventService.GetAllActiveMatchEventAsync();

        var matchEventIsactive = matchEvents.Where(e => e.IsActive).OrderBy(x => x.EventDate);

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

        MatchEvent = matchEventModels;
    }

    public async Task<int> CalculateTotalTicket(int? TotalTickets, int eventId)
    {
        var eventrequest = await _eventRequestService.GetByEventIdAsync(eventId);
        var total = 0;
        if (eventrequest.Count() > 0)
        {
            total = eventrequest.Sum(x => x.ApprovedTickets);
        }
        var totalAvailableTickets =  TotalTickets ?? 0 ;
        var restTotal = Math.Max(0, totalAvailableTickets - total);
        return restTotal;
    }


    public class MatchEventModel
    {
        public int EventId { get; set; }
        public required string EventName { get; set; }
        public string? EventVenue { get; set; }
        public string? EventSession { get; set; }
        public DateTimeOffset? EventDate { get; set; }
        public string? EventThumbnail { get; set; }
        public bool Availability { get; set; }
        public bool IsActive { get; set; }
        public string? EventTime { get; set; }
        public string? EventType { get; set; }
        public int? EventTotalTickets { get; set; }
    }
}
