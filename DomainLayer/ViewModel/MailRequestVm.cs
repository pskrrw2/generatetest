namespace Domain.ViewModel;

public class MailRequestVm
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? ExecutiveName { get; set; }
    public string? ExecutiveEmail { get; set; }
    public string? EventName { get; set; }
    public string? EventDate { get; set; }
    public int? NumberOfTickets { get; set; }
    public string? NumberOfSROTickets { get; set; }
    public int? NumberOfParkingPasses { get; set; }
    public string? UrlPath { get; set; }
    public string? ExpiredDate { get; set; }
    public string? ExpiredBeforeOneDay { get; set; }
    public string? ActionName { get; set; }
}
