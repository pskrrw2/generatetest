using Domain.Entities;

namespace UI.Areas.Shared.Components.Notification;

public class NotificationViewComponentVM
{
    public string? EventName { get; set; }
    public string? ExecutiveName { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }
    public DateTimeOffset? EventDate { get; set; }
    public bool IsAdmin { get; set; }
    public int ApproveRequest { get; set; }
    public int PendingRequest { get; set; }
    public int RejectRequest { get; set; }
    public int RequestId { get; set; }
    public int EventId { get; set; }
    public string? Status { get; set; }
    public int ConferenceId { get; set; }
    public string? SuiteType { get; set; }
    public DateTimeOffset? ConferenceDate { get; set; }
   // public List<Attendee>? Attendees { get; set; }
    public string? PackageSelected { get; set; }
    public string? AddOns { get; set; }
    public int? AttendeeId { get; set; }
}
