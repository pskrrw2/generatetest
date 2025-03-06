namespace Application.Mailing;

public class EmailRequest
{
    public EmailRequest(
        string to,
        string subject,
        string body,
        List<string>? cc = null,
        List<string>? bcc = null,
        Dictionary<string, byte[]>? attachmentData = null,
        Dictionary<string, string>? headers = null)
    {
        To = new List<string> { to };
        Subject = subject;
        Body = body;
        Bcc = bcc ?? new List<string>();
        Cc = cc ?? new List<string>();
        AttachmentData = attachmentData ?? new Dictionary<string, byte[]>();
        Headers = headers ?? new Dictionary<string, string>();
    }

    public EmailRequest(
        List<string> to,
        string subject,
        string? body = null,
        string? from = null,
        string? displayName = null,
        string? replyTo = null,
        string? replyToName = null,
        List<string>? bcc = null,
        List<string>? cc = null,
        IDictionary<string, byte[]>? attachmentData = null,
        IDictionary<string, string>? headers = null)
    {
        To = to;
        Subject = subject;
        Body = body;
        From = from;
        DisplayName = displayName;
        ReplyTo = replyTo;
        ReplyToName = replyToName;
        Bcc = bcc ?? new List<string>();
        Cc = cc ?? new List<string>();
        AttachmentData = attachmentData ?? new Dictionary<string, byte[]>();
        Headers = headers ?? new Dictionary<string, string>();
    }

    public List<string> To { get; set; }

    public string Subject { get; set; }

    public string? Body { get; set; }

    public string? From { get; set; }

    public string? DisplayName { get; set; }

    public string? ReplyTo { get; set; }

    public string? ReplyToName { get; set; }

    public List<string> Bcc { get; set; }

    public List<string> Cc { get; set; }

    public IDictionary<string, byte[]> AttachmentData { get; set; }

    public IDictionary<string, string> Headers { get; set; }
}