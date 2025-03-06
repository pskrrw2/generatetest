using Application.BlobStorage;
using Application.Dto;
using Application.IDataService;
using AspNetCoreHero.ToastNotification.Abstractions;
using Domain.Common.Const;
using Domain.Common.Enum;
using Domain.Common.Enums;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace UI.Areas.Admin.Pages;

[Authorize(Roles = nameof(RoleType.Admin))]
public class CreateorUpdateEventModel(IBlobStorageService blobService, IMatchEventService eventService, INotyfService toastNotification) : PageModel
{
    private readonly IBlobStorageService _blobService = blobService;
    private readonly IMatchEventService _matchEventService = eventService;
    private readonly INotyfService _toastNotification = toastNotification;

    [BindProperty(SupportsGet = true)]
    public int EventId { get; set; }

    [BindProperty]
    public EventModel EventModels { get; set; } = new()
    {
        EventId = 0,
        EventName = string.Empty,
        Venue = string.Empty,
        Image = string.Empty,
        IsActive = true,
    };

    public async Task OnGet(CancellationToken cancellationToken)
    {
        if (EventId is 0)
            return;

        var result = await _matchEventService.GetMatchEventById(EventId);

        var image = Path.GetFileName(result?.EventThumbnail);
        result!.EventThumbnail = image;
        await MatchEventDtoMapping(result);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var eventDto = new MatchEventDto
            {
                EventId = EventId,
                EventName = EventModels.EventName,
                EventDate = EventModels.MatchDate,
                EventTime = EventModels.EventTime,
                EventType = EventModels.EventType.ToString(),
                EventTotalTickets = EventModels.EventTotalTickets,
                EventVenue = EventModels.Venue,
                IsActive = EventModels.IsActive,
                File = EventModels.File,
                EventThumbnail = EventModels.Image!,
                EventSession = EventModels.Season,
                EventTotalParking = EventModels.EventTotalParking,
                EventTotalSROTIckets = EventModels.EventTotalSROTickets,
                EventSROPerTicketPrice = EventModels.EventSROPerTicketPrice,
            };

            var result = EventId != 0 ? await _matchEventService.UpdateMatchEvent(eventDto) : await _matchEventService.AddMatchEventAsync(eventDto);

            if (!result)
                return Page();

            var message = EventId != 0 ? "Event updated successfully" : "Event added successfully";

            _toastNotification.Success(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating match event: {ex.Message}");
        }
        return RedirectToPage(PageNames.MatchEvents);
    }

    private async Task MatchEventDtoMapping(MatchEvent matchEventDto)
    {
        EventModels = new EventModel
        {
            EventId = matchEventDto.EventId,
            EventName = matchEventDto.EventName,
            MatchDate = matchEventDto.EventDate,
            EventTime = matchEventDto.EventTime,
            EventType = GetEventType(matchEventDto.EventType),
            EventTotalTickets = matchEventDto.EventTotalTickets,
            Venue = matchEventDto.EventVenue!,
            IsActive = matchEventDto.IsActive,
            Image = matchEventDto.EventThumbnail!,
            ImageUrl = await _blobService.DisplayImage(matchEventDto.EventThumbnail!),
            Season = matchEventDto.EventSession,
            EventTotalParking = matchEventDto.EventTotalParking,
            EventTotalSROTickets = matchEventDto.EventTotalSROTIckets,
            EventSROPerTicketPrice = matchEventDto.EventSROPerTicketPrice
        };
    }

    private EventType? GetEventType(string? eventTypeStr)
    {
        if (string.IsNullOrEmpty(eventTypeStr))
            return null;

        if (Enum.TryParse(typeof(EventType), eventTypeStr, out var result) && result is EventType eventType)
            return eventType;

        return null;
    }

    public class EventModel
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Please enter event name")]
        [Display(Name = "Event Name")]
        public required string EventName { get; set; }

        [Required(ErrorMessage = "Please enter event venue")]
        [Display(Name = "Event Venue")]
        public required string Venue { get; set; }

        [Required(ErrorMessage = "Please enter event date")]
        [Display(Name = "Event Date")]
        public DateTimeOffset? MatchDate { get; set; }

        [Required(ErrorMessage = "Please add event Image")]
        public required string Image { get; set; }

        [Display(Name = "Is Active")]
        public required bool IsActive { get; set; }

        [Display(Name = "Add Event Image")]
        public IFormFile? File { get; set; }

        [Display(Name = "Season")]
        public string? Season { get; set; }

        [Required(ErrorMessage = "Please enter event time")]
        [Display(Name = "Event Time")]
        public string? EventTime { get; set; }

        [Required(ErrorMessage = "Please select event type")]
        [Display(Name = "Event Type")]
        public EventType? EventType { get; set; } = null;

        [Required(ErrorMessage = "Enter the total number of event tickets")]
        [Display(Name = "Event Total Tickets")]
        public int? EventTotalTickets { get; set; }

        [Required(ErrorMessage = "Enter the total number of Parking passes")]
        [Display(Name = "Event Total Parking")]
        public int? EventTotalParking { get; set; }

        [Required(ErrorMessage = "Enter the total number of SRO tickets")]
        [Display(Name = "Event Total SRO Tickets")]
        public int? EventTotalSROTickets { get; set; }

        [Display(Name = "SRO Per Ticket Price ($)")]
        public int? EventSROPerTicketPrice { get; set; }

        public string? ImageUrl { get; set; }

    }

    public class EventModelValidator : AbstractValidator<EventModel>
    {
        public EventModelValidator()
        {
            RuleFor(x => x.File).Custom((file, context) =>
            {
                if (file == null)
                    return;

                var fileExtension = '.' + file.FileName.Split('.').Last();
                if (Constants.AllowedImageFileTypes.Contains(fileExtension))
                    return;

                context.AddFailure(
                    "Image",
                    $"Only image files are allowed: {string.Join(',', Constants.AllowedImageFileTypes)}");
            });
        }


    }
}
