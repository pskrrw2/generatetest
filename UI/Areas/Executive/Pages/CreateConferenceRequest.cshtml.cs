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
using System.ComponentModel.DataAnnotations;
using static UI.Areas.Executive.Pages.CreateOrUpdateAttendeeModel;
using static UI.Areas.Executive.Pages.CreateRequestModel;

namespace UI.Areas.Executive.Pages;

[Authorize]
public class CreateConferenceRequestModel(IConferenceService conferenceService,
                                            INotyfService toastNotification,
                                            ICurrentUserService currentUserService,
                                            IAddOnMasterService addOnMasterService,
                                            IAttendeeService attendeeService,
                                            IMailManagerService mailManagerService
                                            ) : PageModel
{
    private readonly IConferenceService _conferenceService = conferenceService;
    private readonly INotyfService _toastNotification = toastNotification;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IAddOnMasterService _addOnMasterService = addOnMasterService;
    public readonly IAttendeeService _attendeeService = attendeeService;
    private readonly IMailManagerService _mailManagerService = mailManagerService;

    [BindProperty(SupportsGet = true)]
    public int ConferenceId { get; set; }

    [BindProperty]
    public ConferenceModel ConferenceModels { get; set; } = new()
    {
        ConferenceId = 0,
        SuiteType = string.Empty,
    };

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {

        if (_currentUserService.IsAdmin)
            return RedirectToPage(PageNames.Conferences, new { area = "Executive" });

        if (ConferenceId is 0)
            return Page();

        var result = await _conferenceService.GetById(ConferenceId);
        await MatchConferenceDtoMapping(result);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var conferncesList = await _conferenceService.GetByUserIdAsync(_currentUserService.UserId);
        var isExists = conferncesList.Any(x => x.SuiteType == ConferenceModels.SuiteType && x.ConferenceDate == ConferenceModels.ConferenceDate);

        if (isExists)
        {
            ModelState.AddModelError(string.Empty, "It's a conference that has already been created.");
            return Page();
        }

        var conferenceDto = new ConferenceDto
        {
            Id = ConferenceModels.ConferenceId,
            SuiteType = ConferenceModels.SuiteType!,
            ConferenceDate = ConferenceModels.ConferenceDate.Value,
            ConferenceNotes = ConferenceModels.ConferenceNotes,
            AdminConferenceNotes = _currentUserService.IsAdmin ? ConferenceModels.AdminConferenceNotes : string.Empty,
            CreatedBy = ConferenceModels.ExecutiveId,
            Status = ConferenceModels.Status,
            UserId = ConferenceModels.ExecutiveId,
        };

        var result = ConferenceModels.ConferenceId != 0
                   ? await _conferenceService.Update(conferenceDto)
                   : await _conferenceService.AddAsync(conferenceDto);

        if (!result)
            return Page();

        var message = ConferenceModels.Action switch
        {
            "edit" => "Request updated successfully",
            "add" => "Request created successfully",
            "approved" => "Request approved successfully",
            "reject" => "Request rejected successfully",
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(message))
            await MailingRequesting(conferenceDto, message, ConferenceModels.Action, cancellationToken);

        return RedirectToPage(PageNames.Conferences, new { area = "Executive" });
    }

    public async Task MailingRequesting(ConferenceDto conferenceDto, string message, string action, CancellationToken cancellationToken)
    {
        var userId = !_currentUserService.IsAdmin ? _currentUserService.UserId : conferenceDto.UserId;
        var user = await _currentUserService.GetApplicationUserAsync(userId!);

        var mailConferenceVm = new MailConferenceVm
        {
            Id = conferenceDto.Id,
            ExecutiveName = $"{user.FirstName} {user.LastName}",
            ExecutiveEmail = user.Email,
            ConferenceName = $"{conferenceDto.SuiteType}",
            ConferenceDate = conferenceDto?.ConferenceDate.Date.ToString("MM/dd/yyyy"),
            ExpiredDate = conferenceDto?.ConferenceDate.AddDays(-5).Date.ToString("MM/dd/yyyy"),
            ExpiredBeforeOneDay = conferenceDto?.ConferenceDate.AddDays(-1).Date.ToString("MM/dd/yyyy")
        };

        switch (action.ToLower())
        {
            case "reject":
                await _mailManagerService.SendConferenceRequestRejectedEmailAsync(mailConferenceVm);
                _toastNotification.Warning(message);
                break;

            case "approved":
                await _mailManagerService.SendConferenceRequestApprovedEmailAsync(mailConferenceVm);
                _toastNotification.Success(message);
                break;
            default:
                await _mailManagerService.SendNewConferenceRequestEmailAsync(mailConferenceVm);
                _toastNotification.Success(message);
                break;
        }
    }

    private async Task MatchConferenceDtoMapping(Conference Conference)
    {
        ConferenceModels = new ConferenceModel
        {
            ConferenceId = Conference.ConferenceId,
            SuiteType = Conference.SuiteType!,
            ConferenceDate = Conference.ConferenceDate,
            ConferenceNotes = Conference.ConferenceNotes,
            ExecutiveId = Conference.CreatedBy,
        };
    }

    public async Task<IActionResult> OnGetEditConferenceByIdPartialAsync(int conferenceId, string action, string executiveName)
    {
        var Conference = await _conferenceService.GetById(conferenceId);

        ConferenceModels = new ConferenceModel
        {
            ConferenceId = Conference.ConferenceId,
            SuiteType = Conference.SuiteType!,
            ConferenceDate = Conference.ConferenceDate,
            ConferenceNotes = Conference.ConferenceNotes,
            ExecutiveId = Conference.CreatedBy,
            IsAdmin = _currentUserService.IsAdmin,
            PackageSelected = Conference.PackageSelected,
            AdminConferenceNotes = Conference.AdminConferenceNotes,
            CreatedDate = Conference.CreatedOn,
            ModifiedDate = Conference.LastModifiedOn,
            Status = Conference.Status,
            ExecutiveName = executiveName,
            AddOnDate = Conference.AddOnDate,
            PackageDate = Conference.PackageDate,
            ApproveRejectDate = Conference.ApproveRejectDate
        };

        if (action == "Catering")
        {
            ConferenceModels.SelectedAddOnIds = ParseSelectedAddOnIds(Conference.AddonsSelected);

            ConferenceModels.AddOnMasterModels = await GetAddOnMasterList();
        }
        else if (action == "Guest")
        {
            ConferenceModels.AttendeeModels = await GetAttendeeList(conferenceId);
        }
        else
        {

            ConferenceModels.SelectedAddOnIds = ParseSelectedAddOnIds(Conference.AddonsSelected);
            ConferenceModels.AttendeeModels = await GetAttendeeList(conferenceId);
            ConferenceModels.AddOnMasterModels = await GetAddOnMasterList();
            ConferenceModels.Action = action == "View" ? "View" : " ";
        }

        return action switch
        {
            "View" => Partial("~/Areas/Shared/Modal/Conference/_ViewPartialPageConference.cshtml", ConferenceModels),
          //  "Fill" => Partial("~/Areas/Shared/Modal/Conference/_FillConferenceRequestPartial.cshtml", ConferenceModels),
            "Catering" => Partial("~/Areas/Shared/Modal/Conference/_CateringConferenceRequestPartial.cshtml", ConferenceModels),
            "Guest" => Partial("~/Areas/Shared/Modal/Conference/_AddOrEditConferenceAttendeePartial.cshtml", ConferenceModels),
            _ => Page()
        };
    }

    public async Task<IActionResult> OnGetExportPartialAsync(int conferenceId, string executiveName)
    {
        var Conference = await _conferenceService.GetById(conferenceId);

        ConferenceModels = new ConferenceModel
        {
            ConferenceId = Conference.ConferenceId,
            SuiteType = Conference.SuiteType!,
            ConferenceDate = Conference.ConferenceDate,
            ConferenceNotes = Conference.ConferenceNotes,
            ExecutiveId = Conference.CreatedBy,
            IsAdmin = _currentUserService.IsAdmin,
            PackageSelected = Conference.PackageSelected,
            AdminConferenceNotes = Conference.AdminConferenceNotes,
            CreatedDate = Conference.CreatedOn,
            ModifiedDate = Conference.LastModifiedOn,
            Status = Conference.Status,
            ExecutiveName = executiveName
        };

        ConferenceModels.SelectedAddOnIds = ParseSelectedAddOnIds(Conference.AddonsSelected);
        var selectedAddOnIds = new HashSet<int>();
        if (ConferenceModels.SelectedAddOnIds != null)
        {
            selectedAddOnIds = new HashSet<int>(ConferenceModels.SelectedAddOnIds);
            var addons = await GetAddOnMasterList();
            ConferenceModels.AddOnMasterModels = addons.Where(x => selectedAddOnIds.Contains(x.Id)).ToList();
        }

        ConferenceModels.AttendeeModels = await GetAttendeeList(conferenceId);

        return Partial("~/Areas/Shared/Modal/Conference/_ExportConferenceRequestPartial.cshtml", ConferenceModels);
    }

    public async Task<IActionResult> OnGetExportConferenceToExcelAsync(int conferenceId, string executiveName)
    {
        var Conference = await _conferenceService.GetById(conferenceId);

        var ConferenceModels = new ConferenceModel
        {
            ConferenceId = Conference.ConferenceId,
            SuiteType = Conference.SuiteType!,
            ConferenceDate = Conference.ConferenceDate,
            ConferenceNotes = Conference.ConferenceNotes,
            ExecutiveId = Conference.CreatedBy,
            IsAdmin = _currentUserService.IsAdmin,
            PackageSelected = Conference.PackageSelected,
            AdminConferenceNotes = Conference.AdminConferenceNotes,
            CreatedDate = Conference.CreatedOn,
            ModifiedDate = Conference.LastModifiedOn,
            Status = Conference.Status,
            ExecutiveName = executiveName
        };

        ConferenceModels.SelectedAddOnIds = ParseSelectedAddOnIds(Conference.AddonsSelected);
        var selectedAddOnIds = new HashSet<int>();
        if (ConferenceModels.SelectedAddOnIds != null)
        {
            selectedAddOnIds = new HashSet<int>(ConferenceModels.SelectedAddOnIds);
            var addons = await GetAddOnMasterList();
            ConferenceModels.AddOnMasterModels = addons.Where(x => selectedAddOnIds.Contains(x.Id)).ToList();
        }

        ConferenceModels.AttendeeModels = await GetAttendeeList(conferenceId);

        // EPPlus License Context (needed in newer versions of EPPlus, including 7.4.0)
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // Start generating the Excel file
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Conference Details");

            // Add headers
            worksheet.Cells[1, 1].Value = "Conference ID";
            worksheet.Cells[1, 2].Value = "Suite Type";
            worksheet.Cells[1, 3].Value = "Conference Date";
            worksheet.Cells[1, 4].Value = "Notes";
            worksheet.Cells[1, 5].Value = "Executive Name";
            worksheet.Cells[1, 6].Value = "Status";
            worksheet.Cells[1, 7].Value = "Created Date";
            worksheet.Cells[1, 8].Value = "Modified Date";
            worksheet.Cells[1, 9].Value = "Food Package";

            // Add data
            worksheet.Cells[2, 1].Value = ConferenceModels.ConferenceId;
            worksheet.Cells[2, 2].Value = ConferenceModels.SuiteType;
            worksheet.Cells[2, 3].Value = ConferenceModels.ConferenceDate?.ToString("MM/dd/yyyy");
            worksheet.Cells[2, 4].Value = ConferenceModels.ConferenceNotes;
            worksheet.Cells[2, 5].Value = ConferenceModels.ExecutiveName;
            worksheet.Cells[2, 6].Value = ConferenceModels.Status;
            worksheet.Cells[2, 7].Value = ConferenceModels.CreatedDate;
            worksheet.Cells[2, 8].Value = ConferenceModels.ModifiedDate;
            worksheet.Cells[2, 9].Value = ConferenceModels.PackageSelected;

            // Add attendee data (if applicable)
            int row = 4; // Starting row for attendees
            worksheet.Cells[row, 1].Value = "Attendee Details:";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            row++;

            if (ConferenceModels.AttendeeModels != null && ConferenceModels.AttendeeModels.Any())
            {
                // Add headers for attendees
                worksheet.Cells[row, 1].Value = "Name";
                worksheet.Cells[row, 2].Value = "Email";
                worksheet.Cells[row, 3].Value = "Mobile";
                worksheet.Cells[row, 1, row, 4].Style.Font.Bold = true;
                row++;

                // Add attendee data
                foreach (var attendee in ConferenceModels.AttendeeModels)
                {
                    worksheet.Cells[row, 1].Value = attendee.AttendeeName;
                    worksheet.Cells[row, 2].Value = attendee.AttendeeEmail;
                    worksheet.Cells[row, 3].Value = attendee.AttendeeMobileNumber;
                    row++;
                }
            }

            // Add AddOnMasterModels data
            row++;
            worksheet.Cells[row, 1].Value = "Add-Ons Details:";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            row++;

            if (ConferenceModels.AddOnMasterModels != null && ConferenceModels.AddOnMasterModels.Any())
            {
                worksheet.Cells[row, 1].Value = "ID";
                worksheet.Cells[row, 2].Value = "Item Name";
                worksheet.Cells[row, 3].Value = "Category";
                worksheet.Cells[row, 4].Value = "Price";
                worksheet.Cells[row, 5].Value = "Quantity";
                worksheet.Cells[row, 1, row, 5].Style.Font.Bold = true;
                row++;

                foreach (var addon in ConferenceModels.AddOnMasterModels)
                {
                    worksheet.Cells[row, 1].Value = addon.Id;
                    worksheet.Cells[row, 2].Value = addon.FoodItem; // Assuming FoodItem is the add-on name
                    worksheet.Cells[row, 3].Value = addon.CategoryName;
                    worksheet.Cells[row, 4].Value = addon.Price;
                    worksheet.Cells[row, 5].Value = addon.Quantity;
                    row++;
                }
            }

            // Formatting
            worksheet.Cells[1, 1, 1, 9].Style.Font.Bold = true;
            worksheet.Cells.AutoFitColumns();

            // Prepare to return the file
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            // Return the Excel file
            var fileName = $"{ConferenceModels.ConferenceId}-{ConferenceModels.SuiteType.Replace(" ", "")}-{ConferenceModels.ConferenceDate?.ToString("MM/dd/yyyy").Replace("-", "")}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
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

    public async Task<List<AttendeeModels>> GetAttendeeList(int conferenceId)
    {
        var attendeeList = await _attendeeService.GetByConferenceIdAsync(conferenceId);

        return attendeeList?.Select(item => new AttendeeModels
        {
            AttendeeId = item.Id,
            ConferenceId = item.ConferenceId,
            RequestId = item.RequestId,
            AttendeeName = item.Name,
            AttendeeEmail = item.EmailId,
            AttendeeMobileNumber = item.MobileNumber,
            UserId = item.UserId,
            AttendeeDate = item.LastModifiedOn != null ? item.LastModifiedOn : item.CreatedOn,
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

    public class ConferenceModel
    {
        public int ConferenceId { get; set; }

        [Required(ErrorMessage = "Please select suite type")]
        [Display(Name = "Suite Type")]
        public required string SuiteType { get; set; }

        [Required(ErrorMessage = "Please enter Conference date")]
        [Display(Name = "Conference Date")]
        public DateTimeOffset? ConferenceDate { get; set; }

        [Display(Name = "Conference Notes")]
        public string? ConferenceNotes { get; set; }
        public string? AdminConferenceNotes { get; set; }
        public string? Status { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? Action { get; set; }
        public string? ExecutiveId { get; set; }
        public string? ExecutiveName { get; set; }
        public IEnumerable<AttendeeModels>? AttendeeModels { get; set; }
        public IEnumerable<AddOnMasterModels>? AddOnMasterModels { get; set; }
        public bool IsAdmin { get; set; }
        public string? PackageSelected { get; set; }
        public List<int>? SelectedAddOnIds { get; set; }
        public IEnumerable<Attendee>? Attendees { get; set; }
        public DateTimeOffset? AddOnDate { get; set; }
        public DateTimeOffset? PackageDate { get; set; }
        public DateTimeOffset? ApproveRejectDate { get; set; }
    }

    public class ConferenceModelValidator : AbstractValidator<ConferenceModel>
    {
        public ConferenceModelValidator()
        {
            RuleFor(x => x.ConferenceDate)
                .NotNull().WithMessage("Please enter Conference date");
        }
    }
}
