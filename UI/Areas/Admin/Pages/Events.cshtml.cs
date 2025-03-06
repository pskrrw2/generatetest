using Application.Dto;
using Application.IDataService;
using AspNetCoreHero.ToastNotification.Abstractions;
using Domain.Common.Enum;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Areas.Admin.Pages;

[Authorize(Roles = nameof(RoleType.Admin))]
public class EventsModel(IMatchEventService matchEventService, INotyfService toastNotification) : PageModel
{
    private readonly INotyfService _toastNotification = toastNotification;
    private readonly IMatchEventService _matchEventService = matchEventService;

    [BindProperty]
    public IEnumerable<MatchEvent>? MatchEvent { get; set; }
    public async Task OnGet()
    {
        var matchEvents  = await _matchEventService.GetAllMatchEventAsync();
        MatchEvent = matchEvents.OrderByDescending(x => x.CreatedOn);
    }

    public async Task<IActionResult> OnPostDeleteEventAsync([FromQuery] int eventId, CancellationToken cancellationToken)
    {
        var events = await _matchEventService.GetMatchEventById(eventId);
        await _matchEventService.DeleteMatchEventById(events);

        _toastNotification.Success("Delete Successfully");
        return RedirectToPage(PageNames.MatchEvents);
    }

    public async Task<IActionResult> OnPostIsActiveUserAsync(int eventId, bool isActive)
    {
        var events = await _matchEventService.GetMatchEventById(eventId);
        if (events == null)
        {
            return NotFound();
        }

        var eventDto = new MatchEventDto
        {
            EventId = events.EventId,
            EventName = events.EventName,
            EventDate = events.EventDate,
            EventTime = events.EventTime,
            EventType = events.EventType?.ToString(),
            EventTotalTickets = events.EventTotalTickets,
            EventVenue = events.EventVenue,
            IsActive = isActive,
            EventThumbnail = events.EventThumbnail!,
            EventSession = events.EventSession,
            EventTotalParking = events.EventTotalParking,
            EventTotalSROTIckets = events.EventTotalSROTIckets,
            EventSROPerTicketPrice = events.EventSROPerTicketPrice,
        };

        var result = await _matchEventService.UpdateMatchEvent(eventDto);

        var status = result switch
        {
            true => "success",
            false => "fail"
        };

        return new JsonResult(status);
    }
}
