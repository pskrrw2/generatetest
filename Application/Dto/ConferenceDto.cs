namespace Application.Dto;

public class ConferenceDto
{
    public int Id { get; set; }
    public required string SuiteType { get; set; }
    public required DateTimeOffset ConferenceDate { get; set; }
    public string? ConferenceNotes { get; set; }
    public string? AdminConferenceNotes { get; set; }
    public string? Status { get; set; }
    public string? CreatedBy { get; set; }
    public string? UserId { get; set; }
    public string? PackageSelected { get; set; }
    public string? AddonsSelected { get; set; }
    public DateTimeOffset? AddOnDate { get; set; }
    public DateTimeOffset? PackageDate { get; set; }
    public DateTimeOffset? ApproveRejectDate { get; set; }
}
