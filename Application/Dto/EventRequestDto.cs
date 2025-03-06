namespace Application.Dto;

public class EventRequestDto
{
    public int RequestId { get; set; }
    public DateTimeOffset? EventDate { get; set; }
    public int AppliedParkingPasses { get; set; }
    public string? ExecutiveNotes { get; set; }
    public int AppliedTickets { get; set; }
    public int ApprovedTickets { get; set; }
    public int ApprovedParkingPasses { get; set; }
    public string? AdminNotes { get; set; }
    public bool? SROTickets { get; set; }
    public bool? CateringAndDrinks { get; set; }
    public int? EventId { get; set; }
    public string? Status { get; set; }
    public string? CreatedBy { get; set; }
    public string? SelectedPackage { get; set; }
    public string? AddonsSelected { get; set; }
    public int ApprovedSROTickets { get; set; }
    public int AppliedSROTicket { get; set; }
    public DateTimeOffset? AddOnDate { get; set; }
    public DateTimeOffset? PackageDate { get; set; }
    public DateTimeOffset? ApproveRejectDate { get; set; }
}
