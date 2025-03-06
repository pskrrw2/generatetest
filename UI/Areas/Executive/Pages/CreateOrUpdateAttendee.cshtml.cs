using Application.Dto;
using Application.ICurrentUserService;
using Application.IDataService;
using Application.Mailing;
using AspNetCoreHero.ToastNotification.Abstractions;
using Domain.Common.Const;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using static UI.Areas.Executive.Pages.CreateConferenceRequestModel;
using static UI.Areas.Executive.Pages.CreateRequestModel;

namespace UI.Areas.Executive.Pages;

[Authorize]
public class CreateOrUpdateAttendeeModel(IAttendeeService attendeeService,
                                         IEventRequestService eventRequestService,
                                         IMatchEventService matchEventService,
                                         ICurrentUserService currentUserService,
                                         IMailManagerService mailManagerService,
                                         INotyfService toastNotification,
                                         IConferenceService conferenceService,
                                         IHtmlHelper htmlHelper) : PageModel
{
    public readonly IAttendeeService _attendeeService = attendeeService;
    public readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IEventRequestService _eventRequestService = eventRequestService;
    private readonly IMatchEventService _matchEventService = matchEventService;
    private readonly IMailManagerService _mailManagerService = mailManagerService;
    private readonly INotyfService _toastNotification = toastNotification;
    private readonly IConferenceService _conferenceService = conferenceService;
    private readonly IHtmlHelper _htmlHelper = htmlHelper;

    [BindProperty]
    public RequestModel RequestModels { get; set; } = new()
    {
        RequestId = 0,
        AppliedTickets = 0,
        SROTickets = SROType.No,
        ExecutiveNotes = string.Empty,
        CateringAndDrinks = true,
        AppliedParkingPasses = 0,
        EventId = 0,
    };

    [BindProperty]
    public AttendeeModels? AttendeeModel { get; set; }

    [BindProperty]
    public ConferenceModel ConferenceModels { get; set; } = new()
    {
        ConferenceId = 0,
        SuiteType = string.Empty,
        ConferenceDate = null,
    };

    [BindProperty]
    public IEnumerable<AttendeeModels>? AttendeeModelList { get; set; }
    public async Task OnGet()
    {
        var attendeeList = await _attendeeService.GetByUserIdAsync(_currentUserService.UserId);
        AttendeeModelList = attendeeList?.Select(item => new AttendeeModels
        {
            AttendeeId = item.Id,
            RequestId = item.RequestId,
            AttendeeName = item.Name,
            AttendeeEmail = item.EmailId,
            AttendeeMobileNumber = item.MobileNumber,
            AttendeeTicket = item.TicketsAssigned,
            AttendeeSeatType = GetAttendeeSeatType(item.SeatType),
            UserId = item.UserId
        }).ToList() ?? new List<AttendeeModels>();
    }

    public async Task<IActionResult> OnPostSubmitFormAsync(bool isConference, CancellationToken cancellationToken)
    {
        try
        {
            var selectedIds = isConference ? ConferenceModels.SelectedAddOnIds : RequestModels.SelectedAddOnIds;


            if (isConference)
            {
                var conference = await _conferenceService.GetById(ConferenceModels.ConferenceId);
                var conferenceDto = ConferenceDtoMapping(conference);

                conferenceDto.PackageSelected = ConferenceModels.PackageSelected;
                conferenceDto.AddonsSelected = selectedIds is not null ? string.Join(" ,", selectedIds) : null;
                conferenceDto.Status = nameof(StatusType.Approved);
                var result = await _conferenceService.Update(conferenceDto);

                if (!RequestModels.IsAdmin)
                    await MailingConference(conferenceDto);
            }
            else
            {
                var eventRequest = await _eventRequestService.GetById(RequestModels.RequestId);
                var evntDto = EventRequestDtoMapping(eventRequest);

                evntDto.SelectedPackage = RequestModels.PackageSelected;
                evntDto.AddonsSelected = selectedIds is not null ? string.Join(" ,", selectedIds) : null;
                evntDto.Status = nameof(StatusType.Approved);
                await _eventRequestService.Update(evntDto);

                if (!RequestModels.IsAdmin)
                    await MailingRequesting(evntDto);
            }

            if (RequestModels.IsAdmin)
                _toastNotification.Success("Catering data saved successfully");

            if (isConference)
            {
                return RedirectToPage(PageNames.Conferences, new { area = "Executive" });
            }
            else
            {
                if (RequestModels.IsAdmin)
                    return RedirectToPage(PageNames.ManageRequests, new { area = "Admin" });
                else
                    return RedirectToPage(PageNames.MyRequests, new { area = "Executive" });
            }
        }
        catch (Exception ex)
        {
            // Consider logging the exception instead of just printing it
            return BadRequest("An error occurred while processing the request.");
        }
    }


    public async Task<IActionResult> OnPostGuestSubmitFormAsync(bool isConference, CancellationToken cancellationToken)
    {
        try
        {
            var allOperationsSucceeded = true;
            var attendeeModels = isConference ? ConferenceModels.AttendeeModels : RequestModels.AttendeeModels;
            if (attendeeModels != null)
            {
                foreach (var attendee in attendeeModels)
                {
                    if (!string.IsNullOrEmpty(attendee.AttendeeEmail))
                    {
                        attendee.AttendeeEmail = await Constants.DecryptText(attendee.AttendeeEmail, attendee.Key!, attendee.IV!);
                    }

                    if (!string.IsNullOrEmpty(attendee.AttendeeMobileNumber))
                    {
                        attendee.AttendeeMobileNumber = await Constants.DecryptText(attendee.AttendeeMobileNumber, attendee.Key!, attendee.IV!);
                    }
                }
            }

            var validAttendees = attendeeModels?.Where(x => !string.IsNullOrWhiteSpace(x.AttendeeName)).ToList() ?? new List<AttendeeModels>();

            var attendeesFromDb = isConference
                ? await _attendeeService.GetByConferenceIdAsync(ConferenceModels.ConferenceId)
                : await _attendeeService.GetByRequestIdAsync(RequestModels.RequestId);

            if (attendeesFromDb?.Any() == true)
            {
                var attendeeIdsFromDb = new HashSet<int>(attendeesFromDb.Select(x => x.Id));
                var attendeeIdsFromModel = new HashSet<int>(validAttendees.Select(x => x.AttendeeId));

                var attendeesToDelete = attendeeIdsFromDb.Except(attendeeIdsFromModel).ToList();

                foreach (var id in attendeesToDelete)
                {
                    var attendee = await _attendeeService.GetById(id);
                    if (attendee != null)
                    {
                        await _attendeeService.DeleteById(attendee);
                    }
                }
            }

            if (validAttendees != null)
            {
                foreach (var item in validAttendees)
                {
                    var attendeeDto = new AttendeeDto
                    {
                        RequestId = isConference ? null : RequestModels.RequestId,
                        Id = item.AttendeeId,
                        Name = item.AttendeeName,
                        EmailId = item.AttendeeEmail,
                        MobileNumber = item.AttendeeMobileNumber,
                        NoteToAttendee = item.AttendeeNotes,
                        TicketsAssigned = item.AttendeeTicket,
                        UserId = !RequestModels.IsAdmin ? _currentUserService.UserId : isConference ? ConferenceModels.ExecutiveId : RequestModels.ExecutiveId,
                        AttendeeSeatType = item.AttendeeSeatType,
                        ConferenceId = isConference ? ConferenceModels.ConferenceId : null
                    };

                    bool operationResult = attendeeDto.Id != 0
                        ? await _attendeeService.Update(attendeeDto)
                        : await _attendeeService.AddAsync(attendeeDto);

                    if (!operationResult)
                        allOperationsSucceeded = false;
                }
            }

            if (!allOperationsSucceeded)
                return Page();

            if (RequestModels.IsAdmin)
                _toastNotification.Success("Guest list saved successfully");

            if (isConference)
            {
                if (validAttendees != null)
                {
                    if (!RequestModels.IsAdmin)
                        await MailingAttendeeConference(validAttendees, RequestModels.ExecutiveId!);
                }

                return RedirectToPage(PageNames.Conferences, new { area = "Executive" });
            }
            else
            {
                if (RequestModels.IsAdmin)
                {
                    return RedirectToPage(PageNames.ManageRequests, new { area = "Admin" });
                }
                else
                {
                    if (validAttendees != null)
                    {
                        if (!RequestModels.IsAdmin)
                            await MailingAttendeeRequest(validAttendees, RequestModels.ExecutiveId!);
                    } 

                    return RedirectToPage(PageNames.MyRequests, new { area = "Executive" });
                }
            }
        }
        catch (Exception ex)
        {
            // Consider logging the exception instead of just printing it
            return BadRequest("An error occurred while processing the request.");
        }
    }

    public async Task MailingRequesting(EventRequestDto eventRequestDto)
    {
        var userId = !_currentUserService.IsAdmin ? _currentUserService.UserId : RequestModels.ExecutiveId;
        var user = await _currentUserService.GetApplicationUserAsync(userId!);
        var eventMatch = RequestModels?.EventMatchModels?.FirstOrDefault(x => x.EventId == eventRequestDto.EventId);

        var mailRequestVm = new MailRequestVm
        {
            ExecutiveName = $"{user.FirstName} {user.LastName}",
            ExecutiveEmail = user.Email,
            EventName = eventMatch?.EventName,
            EventDate = eventMatch?.EventDate?.Date.ToString("MM/dd/yyyy"),
            NumberOfTickets = RequestModels.IsAdmin ? RequestModels.ApprovedTickets : RequestModels.AppliedTickets,
            NumberOfParkingPasses = RequestModels.IsAdmin ? RequestModels.ApprovedParkingPasses : RequestModels.AppliedParkingPasses,
            ActionName = "Catering"
        };

        await _mailManagerService.SendRequestFillEmailAsync(mailRequestVm);
        _toastNotification.Success("Catering data saved successfully");
    }

    public async Task MailingConference(ConferenceDto conferenceDto)
    {
        var userId = !_currentUserService.IsAdmin ? _currentUserService.UserId : conferenceDto.CreatedBy;
        var user = await _currentUserService.GetApplicationUserAsync(userId!);

        var mailConferenceVm = new MailConferenceVm
        {
            ExecutiveName = $"{user.FirstName} {user.LastName}",
            ExecutiveEmail = user.Email,
            ConferenceName = $"{conferenceDto.SuiteType}",
            ConferenceDate = conferenceDto?.ConferenceDate.Date.ToString("MM/dd/yyyy"),
            ActionName = "Catering"
        };

        await _mailManagerService.SendConferenceRequestFillEmailAsync(mailConferenceVm);
        _toastNotification.Success("Catering data saved successfully");
    }

    public async Task MailingAttendeeConference(List<AttendeeModels> attendeeDto, string executiveId)
    {
        var userId = !_currentUserService.IsAdmin ? _currentUserService.UserId : executiveId;
        var user = await _currentUserService.GetApplicationUserAsync(userId!);
        var conference = await _conferenceService.GetById(attendeeDto?.FirstOrDefault()?.ConferenceId!);
        var mailConferenceVm = new MailConferenceVm
        {
            ExecutiveName = $"{user.FirstName} {user.LastName}",
            ExecutiveEmail = user.Email,
            ConferenceName = conference.SuiteType,
            ConferenceDate = conference.ConferenceDate.Date.ToString("MM/dd/yyyy"),
            ActionName = "Guest"
        };

        await _mailManagerService.SendConferenceRequestFillEmailAsync(mailConferenceVm);
        _toastNotification.Success("Guest list saved successfully");
    }

    public async Task MailingAttendeeRequest(List<AttendeeModels> attendeeDto, string executiveId)
    {
        var userId = !_currentUserService.IsAdmin ? _currentUserService.UserId : executiveId;
        var user = await _currentUserService.GetApplicationUserAsync(userId!);
        var eventRequest = await _eventRequestService.GetById(attendeeDto?.FirstOrDefault()?.RequestId!);
        var eventMatch = await _matchEventService.GetMatchEventById(eventRequest.EventId);

        var mailRequestVm = new MailRequestVm
        {
            ExecutiveName = $"{user.FirstName} {user.LastName}",
            ExecutiveEmail = user.Email,
            EventName = eventMatch?.EventName,
            EventDate = null,
            NumberOfTickets = 0,
            NumberOfParkingPasses = 0,
            ActionName = "Guest"
        };

        await _mailManagerService.SendRequestFillEmailAsync(mailRequestVm);
        _toastNotification.Success("Guest list saved successfully");
    }

    //public async Task<IActionResult> OnGetEditAttendeePartialAsync(int requestId, string action)
    //{
    //    var attendeeList = await GetAttendeeList(requestId);
    //    return Partial("~/Areas/Shared/Modal/EventRequest/_FillRequestPartial.cshtml", attendeeList);
    //}

    public async Task<IActionResult> OnGetMailAttendeePartialAsync(int Id, bool IsConference)
    {
        var attendee = new AttendeeModels();
        if (IsConference)
            attendee.ConferenceId = Id;
        else
            attendee.RequestId = Id;

        return Partial("~/Areas/Shared/Modal/EventRequest/_MailAttendeeRequestPartial.cshtml", attendee);
    }

    public async Task<IActionResult> OnPostSubmitMailFormAsync([FromForm] AttendeeModels attendeeModels, CancellationToken cancellationToken)
    {
        try
        {
            var attendeeList = attendeeModels.ConferenceId != null
            ? await _attendeeService.GetByConferenceIdAsync(attendeeModels.ConferenceId.Value)
            : await _attendeeService.GetByRequestIdAsync(attendeeModels.RequestId!.Value);

            if (attendeeList != null)
            {
                var attendEmailList = attendeeList.Select(x => x.EmailId).Where(email => !string.IsNullOrEmpty(email)).ToList();
                var executiveUserId = attendeeList?.FirstOrDefault()?.UserId;
                var executive = await _currentUserService.GetApplicationUserAsync(executiveUserId!);

                attendEmailList.Add(executive?.Email);

                var notes = _htmlHelper.Raw(attendeeModels.AttendeeNotes);

                var attachments = attendeeModels.File != null ? ConvertFormFilesToDictionary(attendeeModels.File!) : null;
                var attnedmailVm = new AttendeeEmailVm
                {
                    Email = attendEmailList!,
                    Attachments = attachments,
                    ExecutiveMail = executive?.Email,
                    Message = notes,
                    Subject = attendeeModels.Subject
                };

                await _mailManagerService.SendAttendeeQueryEmailAsync(attnedmailVm);
                _toastNotification.Success("Notification has been sent successfully to attendees on their email");
            }

            if (attendeeModels.ConferenceId == null)
                return RedirectToPage(PageNames.ManageRequests, new { area = "Admin" });
            else
                return RedirectToPage(PageNames.Conferences, new { area = "Executive" });

        }
        catch (Exception ex)
        {
            return BadRequest("An error occurred while processing the request.");
        }
    }

    public IDictionary<string, byte[]> ConvertFormFilesToDictionary(List<IFormFile> files)
    {
        var attachmentData = new Dictionary<string, byte[]>();

        foreach (var file in files)
        {
            if (file != null && file.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Copy the file content to the memory stream
                    file.CopyTo(memoryStream);

                    // Add the file name and byte array to the dictionary
                    attachmentData.Add(file.FileName, memoryStream.ToArray());
                }
            }
        }

        return attachmentData;
    }


    public async Task<IEnumerable<AttendeeModels>> GetAttendeeList(int requestId)
    {
        var attendeeList = await _attendeeService.GetByRequestIdAsync(requestId);

        return attendeeList?.Select(item => new AttendeeModels
        {
            AttendeeId = item.Id,
            RequestId = item.RequestId,
            AttendeeName = item.Name,
            AttendeeEmail = item.EmailId,
            AttendeeMobileNumber = item.MobileNumber,
            AttendeeTicket = item.TicketsAssigned,
            AttendeeNotes = item.NoteToAttendee,
            AttendeeSeatType = GetAttendeeSeatType(item.SeatType),
            UserId = item.UserId,
            ConferenceId = item.ConferenceId,
        }).ToList() ?? new List<AttendeeModels>();
    }

    private SeatType? GetAttendeeSeatType(string? seatypeStr)
    {
        if (string.IsNullOrEmpty(seatypeStr))
            return null;

        if (Enum.TryParse(typeof(SeatType), seatypeStr, out var result) && result is SeatType seatType)
            return seatType;

        return null;
    }

    private EventRequestDto EventRequestDtoMapping(EventRequest eventRequest)
    {
        return new EventRequestDto
        {
            EventId = eventRequest.EventId,
            RequestId = eventRequest.RequestId,
            AppliedParkingPasses = eventRequest.AppliedParkingPasses!,
            ExecutiveNotes = eventRequest.ExecutiveNotes,
            AppliedTickets = eventRequest.AppliedTickets!,
            ApprovedTickets = eventRequest.ApprovedTickets,
            SROTickets = eventRequest.SROTickets,
            CateringAndDrinks = eventRequest.CateringAndDrinks,
            Status = _currentUserService.IsAdmin ? eventRequest.Status : nameof(StatusType.Pending),
            CreatedBy = !(_currentUserService.IsAdmin) ? _currentUserService.UserId : eventRequest.CreatedBy,
            ApprovedParkingPasses = eventRequest.ApprovedParkingPasses,
            AdminNotes = eventRequest.AdminNotes ?? string.Empty,
            ApprovedSROTickets = eventRequest.ApprovedSROTickets ?? 0,
            AppliedSROTicket = eventRequest.AppliedSROTickets ?? 0
        };
    }
    private ConferenceDto ConferenceDtoMapping(Conference conference)
    {
        return new ConferenceDto
        {
            Id = conference.ConferenceId,
            SuiteType = conference.SuiteType!,
            ConferenceDate = conference.ConferenceDate,
            ConferenceNotes = conference.ConferenceNotes,
            AdminConferenceNotes = _currentUserService.IsAdmin ? conference.AdminConferenceNotes : string.Empty,
            CreatedBy = conference.CreatedBy,
            Status = conference.Status,
            UserId = conference.UserId,
        };
    }

    public class AttendeeModels
    {
        public int AttendeeId { get; set; }
        public int? RequestId { get; set; }
        public string? AttendeeName { get; set; }
        public string? AttendeeEmail { get; set; }
        public string? AttendeeMobileNumber { get; set; }
        public int? AttendeeTicket { get; set; }
        public string? AttendeeNotes { get; set; }
        public SeatType? AttendeeSeatType { get; set; }
        public string? UserId { get; set; }
        public string? Subject { get; set; }
        public int? ConferenceId { get; set; }
        public List<IFormFile>? File { get; set; }
        public DateTimeOffset? AttendeeDate { get; set; }
        public string? Key { get; set; }
        public string? IV { get; set; }
    }

}
