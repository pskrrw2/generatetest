using Microsoft.AspNetCore.Html;

namespace Domain.ViewModel;

public class AttendeeEmailVm
{
    public string? Title { get; set; }
    public string? Name { get; set; }
    public string? ExecutiveMail { get; set; }
    public string? Subject { get; set; }
    public IHtmlContent? Message { get; set; }
    public string? UrlPath { get; set; }
    public List<string>? Email { get; set; }
    public IDictionary<string, byte[]>? Attachments { get; set; }
}
