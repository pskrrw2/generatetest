namespace Infrastructure.Mailing;
public class SendGridMailSenderOptions
{
    public string? ApiKey { get; set; }
    public string? SenderEmail { get; set; }
    public string? SenderName { get; set; }
}
