namespace Domain.ViewModel;

public class MailConferenceVm
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? ExecutiveName { get; set; }
    public string? ExecutiveEmail { get; set; }
    public string? ConferenceName { get; set; }
    public string? ConferenceDate { get; set; }
    public string? UrlPath { get; set; }
    public string? ExpiredDate { get; set; }
    public string? ExpiredBeforeOneDay { get; set; }
    public string? ActionName { get; set; }
}
