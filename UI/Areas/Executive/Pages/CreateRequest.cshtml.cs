using Application.Dto;
using Application.ICurrentUserService;
using Application.IDataService;
using Application.Mailing;
using AspNetCoreHero.ToastNotification.Abstractions;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.ViewModel;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OfficeOpenXml;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static UI.Areas.Executive.Pages.CreateOrUpdateAttendeeModel;

namespace UI.Areas.Executive.Pages;

[Authorize]
public class CreateRequestModel(IMatchEventService matchEventService,
    IEventRequestService eventRequestService,
    ICurrentUserService currentUserService,
    INotyfService toastNotification,
    IAttendeeService attendeeService,
    IMailManagerService mailManagerService,
    IAddOnMasterService addOnMasterService) : PageModel
{
    private readonly IMatchEventService _matchEventService = matchEventService;
    private readonly IEventRequestService _eventRequestService = eventRequestService;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly INotyfService _toastNotification = toastNotification;
    public readonly IAttendeeService _attendeeService = attendeeService;
    private readonly IMailManagerService _mailManagerService = mailManagerService;
    private readonly IAddOnMasterService _addOnMasterService = addOnMasterService;

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

    public async Task<IActionResult> OnGetEventRequestAsync(CancellationToken cancellationToken)
    {
        RequestModels.EventMatchModels = await GetEventMatchModelsAsync(string.Empty);
        return Partial("~/Areas/Shared/Modal/_CreateRequestpartial.cshtml", RequestModels);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        RequestModels.EventMatchModels = await GetEventMatchModelsAsync(string.Empty);
        if (RequestModels.AppliedTickets == 0)
            ModelState.Remove(nameof(RequestModels.AppliedTickets));

        if (RequestModels.AppliedParkingPasses == 0)
            ModelState.Remove(nameof(RequestModels.AppliedParkingPasses));

        if (RequestModels.AppliedSROTicket == 0)
            ModelState.Remove(nameof(RequestModels.AppliedSROTicket));

        if (!ModelState.IsValid)
            return NotFound();

        var eventDto = new EventRequestDto
        {
            RequestId = RequestModels.RequestId,
            EventId = RequestModels.EventId,
            AppliedTickets = RequestModels.AppliedTickets ?? 0,
            SROTickets = RequestModels.SROTickets == SROType.Yes,
            ExecutiveNotes = RequestModels.ExecutiveNotes,
            CateringAndDrinks = RequestModels.CateringAndDrinks,
            AppliedParkingPasses = RequestModels.AppliedParkingPasses,
            ApprovedTickets = RequestModels.ApprovedTickets ?? 0,
            ApprovedParkingPasses = RequestModels.ApprovedParkingPasses,
            AdminNotes = RequestModels.AdminNotes,
            Status = RequestModels.Status,
            CreatedBy = RequestModels.ExecutiveId,
            EventDate = RequestModels.EventDate,
            AppliedSROTicket = RequestModels.SROTickets == SROType.Yes ? RequestModels.AppliedSROTicket : 0,
            ApprovedSROTickets = RequestModels.ApprovedSROTickets
        };

        var result = RequestModels.RequestId is not 0
           ? await _eventRequestService.Update(eventDto)
           : await _eventRequestService.AddAsync(eventDto);

        if (!result)
            return Page();

        var message = RequestModels.action switch
        {
            "edit" => "Request updated successfully",
            "add" => "Request created successfully",
            "approved" => "Request approved successfully",
            "reject" => "Request rejected successfully",
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(message))
            await MailingRequesting(eventDto, message, RequestModels.action!, cancellationToken);

        return _currentUserService.IsAdmin
        ? RedirectToPage(PageNames.ManageRequests, new { area = "Admin" })
        : RedirectToPage(PageNames.MyRequests, new { area = "Executive" });
    }

    public async Task<IActionResult> OnGetEditRequestByIdPartialAsync(int requestId, string action)
    {
        var eventRequest = await _eventRequestService.GetById(requestId);
        var modelEventList = await GetEventMatchModelsAsync(action);

        var model = await MappingModel(eventRequest, modelEventList);

        model.ExecutiveId = eventRequest.CreatedBy;
        if (action == "Catering")
        {
            model.SelectedAddOnIds = ParseSelectedAddOnIds(eventRequest.AddonsSelected);
            model.AddOnMasterModels = await GetAddOnMasterList();
        }
        else if (action == "Guest")
        {
            model.AttendeeModels = await GetAttendeeList(requestId);
        }
        else
        {
            model.SelectedAddOnIds = ParseSelectedAddOnIds(eventRequest.AddonsSelected);
            model.AddOnMasterModels = await GetAddOnMasterList();
            model.AttendeeModels = await GetAttendeeList(requestId);
            model.action = action == "View" ? "View" : "";
        }

        return action switch
        {
            "Edit" => Partial("~/Areas/Shared/Modal/_CreateRequestPartial.cshtml", model),
            "View" => Partial("~/Areas/Shared/Modal/EventRequest/_ViewPartialPageRequest.cshtml", model),
            //  "Fill" => Partial("~/Areas/Shared/Modal/EventRequest/_FillRequestPartial.cshtml", model),
            "Catering" => Partial("~/Areas/Shared/Modal/EventRequest/_CateringRequestPartial.cshtml", model),
            "Guest" => Partial("~/Areas/Shared/Modal/EventRequest/_AddOrEditAttendeePartial.cshtml", model),
            _ => Page()
        };
    }

    public async Task<IActionResult> OnGetExportPartialAsync(int requestId)
    {
        var eventRequest = await _eventRequestService.GetById(requestId);
        if (eventRequest == null) return NotFound();

        var modelEventList = await GetEventMatchModelsAsync(string.Empty);

        var model = await MappingModel(eventRequest, modelEventList);

        model.AttendeeModels = await GetAttendeeList(requestId);

        model.SelectedAddOnIds = ParseSelectedAddOnIds(eventRequest.AddonsSelected ?? string.Empty);
        var selectedAddOnIds = new HashSet<int>();
        if (model.SelectedAddOnIds != null)
        {
            selectedAddOnIds = new HashSet<int>(model.SelectedAddOnIds);
            var addons = await GetAddOnMasterList();
            model.AddOnMasterModels = addons.Where(x => selectedAddOnIds.Contains(x.Id)).ToList();
        }

        return Partial("~/Areas/Shared/Modal/EventRequest/_ExportEventRequestPartial.cshtml", model);
    }

    public async Task<IActionResult> OnGetExportRequestToExcelAsync(int requestId)
    {
        var eventRequest = await _eventRequestService.GetById(requestId);
        if (eventRequest == null) return NotFound();

        var modelEventList = await GetEventMatchModelsAsync(string.Empty);

        var model = await MappingModel(eventRequest, modelEventList);

        model.AttendeeModels = await GetAttendeeList(requestId);

        model.SelectedAddOnIds = ParseSelectedAddOnIds(eventRequest.AddonsSelected ?? string.Empty);
        var selectedAddOnIds = new HashSet<int>();
        if (model.SelectedAddOnIds != null)
        {
            selectedAddOnIds = new HashSet<int>(model.SelectedAddOnIds);
            var addons = await GetAddOnMasterList();
            model.AddOnMasterModels = addons.Where(x => selectedAddOnIds.Contains(x.Id)).ToList();
        }

        // EPPlus License Context
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        // Generate Excel File
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Event Request Details");

            // Add Event Request Details
            worksheet.Cells[1, 1].Value = "Request ID";
            worksheet.Cells[1, 2].Value = "Event Name";
            worksheet.Cells[1, 3].Value = "Executive Name";
            worksheet.Cells[1, 4].Value = "Applied Parking Passes";
            worksheet.Cells[1, 5].Value = "Approved Parking Passes";
            worksheet.Cells[1, 6].Value = "Applied Tickets";
            worksheet.Cells[1, 7].Value = "Approved Tickets";
            worksheet.Cells[1, 8].Value = "SRO Tickets";
            worksheet.Cells[1, 9].Value = "Applied SRO Ticket";
            worksheet.Cells[1, 10].Value = "Approved SRO Ticket";
            worksheet.Cells[1, 11].Value = "Food Package";
            worksheet.Cells[1, 12].Value = "Status";
            worksheet.Cells[1, 13].Value = "Created Date";

            worksheet.Cells[2, 1].Value = model.RequestId;
            worksheet.Cells[2, 2].Value = model.EventName;
            worksheet.Cells[2, 3].Value = model.ExecutiveName;
            worksheet.Cells[2, 4].Value = model.AppliedParkingPasses;
            worksheet.Cells[2, 5].Value = model.ApprovedParkingPasses;
            worksheet.Cells[2, 6].Value = model.AppliedTickets;
            worksheet.Cells[2, 7].Value = model.ApprovedTickets;
            worksheet.Cells[2, 8].Value = model.SROTickets;
            worksheet.Cells[2, 9].Value = model.AppliedSROTicket;
            worksheet.Cells[2, 10].Value = model.ApprovedSROTickets;
            worksheet.Cells[2, 11].Value = model.PackageSelected;
            worksheet.Cells[2, 12].Value = model.Status;
            worksheet.Cells[2, 13].Value = model.CreatedDate?.ToString("MM/dd/yyyy");

            // Add Attendee Details Header and Bold Styling
            int row = 5;
            worksheet.Cells[row, 1].Value = "Attendee Details:";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            row++;

            // Attendee Data Headers
            worksheet.Cells[row, 1].Value = "Name";
            worksheet.Cells[row, 2].Value = "Email";
            worksheet.Cells[row, 3].Value = "Mobile";
            worksheet.Cells[row, 4].Value = "Ticket";
            worksheet.Cells[row, 5].Value = "Seat Type";
            worksheet.Cells[row, 1, row, 5].Style.Font.Bold = true;
            row++;

            // Attendee Data
            if (model.AttendeeModels != null)
            {
                foreach (var attendee in model.AttendeeModels)
                {
                    worksheet.Cells[row, 1].Value = attendee.AttendeeName;
                    worksheet.Cells[row, 2].Value = attendee.AttendeeEmail;
                    worksheet.Cells[row, 3].Value = attendee.AttendeeMobileNumber;
                    worksheet.Cells[row, 4].Value = attendee.AttendeeTicket;
                    worksheet.Cells[row, 5].Value = attendee.AttendeeSeatType;
                    row++;
                }
            }

            // Add-On Details Header and Bold Styling
            row++;
            worksheet.Cells[row, 1].Value = "Add-Ons Details:";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            row++;

            // Add-On Data Headers
            worksheet.Cells[row, 1].Value = "ID";
            worksheet.Cells[row, 2].Value = "Food Item";
            worksheet.Cells[row, 3].Value = "Category";
            worksheet.Cells[row, 4].Value = "Price";
            worksheet.Cells[row, 5].Value = "Quantity";
            worksheet.Cells[row, 1, row, 5].Style.Font.Bold = true;
            row++;

            // Add-On Data
            if (model.AddOnMasterModels != null)
            {
                foreach (var addon in model.AddOnMasterModels)
                {
                    worksheet.Cells[row, 1].Value = addon.Id;
                    worksheet.Cells[row, 2].Value = addon.FoodItem; // Assuming FoodItem is the name
                    worksheet.Cells[row, 3].Value = addon.CategoryName;
                    worksheet.Cells[row, 4].Value = addon.Price;
                    worksheet.Cells[row, 5].Value = addon.Quantity;
                    row++;
                }
            }

            // Auto-fit columns and finalize Excel file
            worksheet.Cells[1, 1, 1, 13].Style.Font.Bold = true;
            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"{model.RequestId}-{model?.EventName?.Replace(" ", "").Replace(".", "")}-{model?.ExecutiveName?.Replace(" ", "")}-{model?.CreatedDate?.ToString("MM/dd/yyyy").Replace("/", "")}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }



    // Helper method to parse selected AddOn IDs
    public static List<int>? ParseSelectedAddOnIds(string? addonsSelected) =>
    string.IsNullOrWhiteSpace(addonsSelected)
        ? null
        : addonsSelected
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();

    public async Task MailingRequesting(EventRequestDto eventRequestDto, string message, string action, CancellationToken cancellationToken)
    {
        var userId = !_currentUserService.IsAdmin ? _currentUserService.UserId : RequestModels.ExecutiveId;
        var user = await _currentUserService.GetApplicationUserAsync(userId!);
        var eventMatch = RequestModels?.EventMatchModels?.FirstOrDefault(x => x.EventId == eventRequestDto.EventId);

        var mailRequestVm = new MailRequestVm
        {
            Id = eventRequestDto.RequestId,
            ExecutiveName = $"{user.FirstName} {user.LastName}",
            ExecutiveEmail = user.Email,
            EventName = eventMatch?.EventName,
            EventDate = eventMatch?.EventDate?.Date.ToString("MM/dd/yyyy"),
            ExpiredBeforeOneDay = eventMatch?.EventDate?.AddDays(-1).Date.ToString("MM/dd/yyyy"),
            NumberOfTickets = eventRequestDto.AppliedTickets,
            NumberOfSROTickets = eventRequestDto.AppliedSROTicket.ToString() ?? "N/A",
            NumberOfParkingPasses = eventRequestDto.AppliedParkingPasses,
            ExpiredDate = eventMatch?.EventDate?.AddDays(-5).Date.ToString("MM/dd/yyyy")
        };

        switch (action.ToLower())
        {
            case "reject":
                await _mailManagerService.SendRequestRejectedEmailAsync(mailRequestVm);
                _toastNotification.Warning(message);
                break;

            case "approved":
                {
                    mailRequestVm.NumberOfTickets = eventRequestDto.ApprovedTickets;
                    mailRequestVm.NumberOfSROTickets = eventRequestDto.ApprovedSROTickets.ToString() ?? "N/A";
                    mailRequestVm.NumberOfParkingPasses = eventRequestDto.ApprovedParkingPasses;
                    await _mailManagerService.SendRequestApprovedEmailAsync(mailRequestVm);
                    _toastNotification.Success(message);
                    break;
                }
            default:
                {
                    await _mailManagerService.SendNewRequestCreatedEmailAsync(mailRequestVm);
                    _toastNotification.Success(message);
                    break;
                }
        }
    }

    public async Task<IActionResult> OnGetDashboardPartialAsync(int eventId)
    {
        var eventMatches = await GetEventMatchModelsAsync(string.Empty);
        var events = eventMatches.ToList();
        var eventV = events.FirstOrDefault(e => e.EventId == eventId);

        var model = new RequestModel
        {
            EventId = eventId,
            RequestId = 0,
            AppliedParkingPasses = 0,
            AppliedTickets = 0,
            SROTickets = SROType.No,
            CateringAndDrinks = false,
            EventMatchModels = eventMatches,
            AvailableSeat = eventV != null ? eventV.AvailableSeat : eventV.EventTotalTickets,
            AvailableParking = eventV.AvailableParking,
            AvailableSROSeat = eventV.AvailableSROSeat,
            AppliedSROTicket = 0,
            SROperTicketPrice = eventV.SROperTicketPrice
        };

        return Partial("~/Areas/Shared/Modal/_CreateRequestpartial.cshtml", model);
    }

    public async Task<List<AttendeeModels>> GetAttendeeList(int requestId)
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
            AttendeeSeatType = GetAttendeeSeatType(item.SeatType),
            UserId = item.UserId,
            AttendeeDate = item.LastModifiedOn != null ? item.LastModifiedOn : item.CreatedOn,
        }).ToList() ?? new List<AttendeeModels>();
    }

    public async Task<List<AddOnMasterModels>> GetAddOnMasterList()
    {
        var addOnMasterList = await _addOnMasterService.GetAllAsync();

        return addOnMasterList?.Select(item => new AddOnMasterModels
        {
            Id = item.Id,
            Menu = item.Menu,
            CategoryName = item.CategoryName,
            FoodType = item.FoodType,
            FoodItem = item.FoodItem,
            Price = item.Price,
            Quantity = item.Quantity
        }).ToList() ?? new List<AddOnMasterModels>();
    }

    public async Task<IEnumerable<EventMatchModels>> GetEventMatchModelsAsync(string action)
    {
        var isUpcomingEvent = DateTimeOffset.UtcNow;
        // Fetch event requests and matches asynchronously
        var eventRequests = await _eventRequestService.GetAllAsync();
        var eventMatches = await _matchEventService.GetAllActiveMatchEventAsync();

        // Project event matches into EventMatchModels
        var eventMatchModels = eventMatches.Select(matchEvent =>
        {
            // Calculate total approved tickets and parking for the current event
            var (totalApprovedTickets, totalApprovedParking, totalApprovedSROTickets) = CalculateTotals(matchEvent.EventId, eventRequests);

            // Create and return the EventMatchModels object
            return new EventMatchModels
            {
                EventId = matchEvent.EventId,
                EventName = matchEvent.EventName,
                EventDate = matchEvent.EventDate,
                AvailableSeat = CalculateAvailability(matchEvent.EventTotalTickets ?? 0, totalApprovedTickets),
                AvailableParking = CalculateAvailability(matchEvent.EventTotalParking ?? 0, totalApprovedParking),
                AvailableSROSeat = CalculateAvailability(matchEvent.EventTotalSROTIckets ?? 0, totalApprovedSROTickets),
                EventTotalTickets = matchEvent.EventTotalTickets ?? 0,
                Image = matchEvent.EventThumbnail,
                SROperTicketPrice = matchEvent.EventSROPerTicketPrice
            };
        }).ToList();

        if (action == "View")
            return eventMatchModels;
        else
            return eventMatchModels.Where(x => x.EventDate > isUpcomingEvent);

        // Local function to calculate total approved tickets, parking and SRO tickets for an event
        (int tickets, int parking, int sro) CalculateTotals(int eventId, IEnumerable<EventRequest> requests)
        {
            var totalTickets = requests.Where(req => req.EventId == eventId)
                                       .Sum(req => req.ApprovedTickets);
            var totalParking = requests.Where(req => req.EventId == eventId)
                                       .Sum(req => req.ApprovedParkingPasses);
            var totalSROTickets = requests.Where(req => req.EventId == eventId)
                                       .Sum(req => req.ApprovedSROTickets);
            return (totalTickets, totalParking, totalSROTickets.Value);
        }

        // Local function to calculate available seats or parking
        int CalculateAvailability(int totalCapacity, int totalApproved)
            => Math.Max(0, totalCapacity - totalApproved);
    }


    public async Task<RequestModel> MappingModel(EventRequest eventRequest, IEnumerable<EventMatchModels> matchEvents)
    {
        var userId = !_currentUserService.IsAdmin ? _currentUserService.UserId : eventRequest.CreatedBy;
        var user = await _currentUserService.GetApplicationUserAsync(userId!);

        return new RequestModel
        {
            EventId = eventRequest.EventId,
            RequestId = eventRequest.RequestId,
            AppliedParkingPasses = eventRequest.AppliedParkingPasses,
            AppliedTickets = eventRequest.AppliedTickets,
            SROTickets = (bool)eventRequest.SROTickets! ? SROType.Yes : SROType.No,
            CateringAndDrinks = eventRequest.CateringAndDrinks.HasValue,
            ExecutiveNotes = eventRequest.ExecutiveNotes,
            EventMatchModels = matchEvents,
            CreatedDate = eventRequest.CreatedOn,
            ModifiedDate = eventRequest.LastModifiedOn,
            IsAdmin = _currentUserService.IsAdmin ? true : false,
            Status = eventRequest.Status,
            ApprovedTickets = eventRequest.ApprovedTickets,
            ApprovedParkingPasses = eventRequest.ApprovedParkingPasses,
            AdminNotes = eventRequest.AdminNotes,
            PackageSelected = eventRequest.PackageSelected,
            AppliedSROTicket = eventRequest.AppliedSROTickets ?? 0,
            ApprovedSROTickets = eventRequest.ApprovedSROTickets ?? 0,
            ExecutiveName = $"{user.FirstName} {user.LastName}",
            EventDate = matchEvents?.FirstOrDefault(x => x.EventId == eventRequest.EventId)?.EventDate,
            ExecutiveId = user.Id,
            Image = matchEvents?.FirstOrDefault(x => x.EventId == eventRequest.EventId)?.Image,
            EventName = matchEvents?.FirstOrDefault(x => x.EventId == eventRequest.EventId)?.EventName,
            PackageDate = eventRequest.PackageDate,
            AddOnDate = eventRequest.AddOnDate,
            ApproveRejectDate = eventRequest.ApproveRejectDate
        };
    }

    private SeatType? GetAttendeeSeatType(string? seatypeStr)
    {
        if (string.IsNullOrEmpty(seatypeStr))
            return null;

        if (Enum.TryParse(typeof(SeatType), seatypeStr, out var result) && result is SeatType seatType)
            return seatType;

        return null;
    }

    public class RequestModel
    {
        public int RequestId { get; set; }

        [DisplayName("Number of Parking Passes")]
        public int AppliedParkingPasses { get; set; }
        public string? ExecutiveNotes { get; set; }

        [DisplayName("Number of Ticket")]
        //[Range(1, 20, ErrorMessage = "The {0} must be between {1} and {2}.")]
        public int? AppliedTickets { get; set; }

        public int? ApprovedTickets { get; set; }
        public int ApprovedParkingPasses { get; set; }
        public string? AdminNotes { get; set; }

        [DisplayName("SRO Pass")]
        public SROType SROTickets { get; set; } = SROType.No;

        [DisplayName("Apply for addons such as catering and drinks")]
        public bool CateringAndDrinks { get; set; } = true;

        [Required(ErrorMessage = "Please select an event")]
        public required int EventId { get; set; }
        public string? Status { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public IEnumerable<EventMatchModels>? EventMatchModels { get; set; }
        public IEnumerable<AttendeeModels>? AttendeeModels { get; set; }
        public IEnumerable<AddOnMasterModels>? AddOnMasterModels { get; set; }
        public string? EventName { get; set; }
        public string? ExecutiveName { get; set; }
        public string? ExecutiveId { get; set; }
        public int AvailableSeat { get; set; }
        public int AvailableParking { get; set; }
        public int AvailableSROSeat { get; set; }
        public bool IsAdmin { get; set; }
        public string? action { get; set; }
        public string? PackageSelected { get; set; }
        public DateTimeOffset? EventDate { get; set; }
        public List<int>? SelectedAddOnIds { get; set; }
        public int ApprovedSROTickets { get; set; }

        [DisplayName("Number of SRO Ticket")]
        //[Range(1, 12, ErrorMessage = "The {0} must be between {1} and {2}.")]
        public int AppliedSROTicket { get; set; }
        public string? AddOnSelected { get; set; }
        public string? Image { get; set; }
        public int? SROperTicketPrice { get; set; }
        public DateTimeOffset? AddOnDate { get; set; }
        public DateTimeOffset? PackageDate { get; set; }
        public DateTimeOffset? ApproveRejectDate { get; set; }
    }

    public class EventMatchModels
    {
        public int AvailableSeat { get; set; }
        public int AvailableParking { get; set; }
        public int AvailableSROSeat { get; set; }
        public string? EventName { get; set; }
        public int? EventId { get; set; }
        public DateTimeOffset? EventDate { get; set; }
        public int EventTotalTickets { get; set; }
        public string? Image { get; set; }
        public int? SROperTicketPrice { get; set; }
    }

    public class AddOnMasterModels
    {
        public int Id { get; set; }
        public string? Menu { get; set; }
        public string? CategoryName { get; set; }
        public string? FoodType { get; set; }
        public string? FoodItem { get; set; }
        public string? Price { get; set; }
        public int Quantity { get; set; }
    }

    public class RequestModelValidator : AbstractValidator<RequestModel>
    {
        public RequestModelValidator()
        {
            RuleFor(x => x.EventId)
                .NotNull().WithMessage("Please select event name");

            RuleFor(x => x.AppliedTickets)
                .NotNull().WithMessage("Please select number of passes")
                .GreaterThan(0).WithMessage("Please select number of passes");

            RuleFor(x => x.AppliedParkingPasses)
                .NotNull().WithMessage("Please select number of parking pass");

            RuleFor(x => x.AppliedSROTicket)
                .NotNull().When(x => x.SROTickets == SROType.Yes)
                .WithMessage("Please select number of SRO passes");
        }
    }
}
