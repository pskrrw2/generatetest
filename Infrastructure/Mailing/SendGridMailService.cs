using Application.Mailing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Infrastructure.Mailing;

public class SendGridMailService : IMailService
{
    private readonly ILogger<SendGridMailService> _logger;
    private readonly SendGridMailSenderOptions _options;

    public SendGridMailService(IOptions<SendGridMailSenderOptions> options, ILogger<SendGridMailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(EmailRequest request)
    {
        try
        {
            var response = await ExecuteAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
    }

    private async Task<Response> ExecuteAsync(EmailRequest request)
    {
        var client = new SendGridClient(_options.ApiKey);

        var msg = CreateMessage(request);

        foreach (var s in request.To)
            msg.AddTo(new EmailAddress(s));

        return await client.SendEmailAsync(msg);
    }

    private SendGridMessage CreateMessage(EmailRequest request)
    {
        var subject = request.Subject;

        var message = new SendGridMessage
        {
            From = new EmailAddress(_options.SenderEmail, _options.SenderName),
            Subject = subject,
            PlainTextContent = request.Body,
            HtmlContent = request.Body,
        };

        if (request.Bcc != null && request.Bcc.Any())
            AddBccsIfNeeded(request, message);

        if (request.AttachmentData != null && request.AttachmentData.Any())
            AddAttachments(request, message);

        return message;
    }

    private static void AddBccsIfNeeded(EmailRequest request, SendGridMessage msg)
    {
        var bccs = request?.Bcc?.Select(x => new EmailAddress(x))?.ToList();
        bccs.RemoveAll(x => bccs.Exists(y => request.To.Contains(y.Email))).ToString();
        if (bccs?.Count > 0)
            msg.AddBccs(bccs);
    }

    private static void AddAttachments(EmailRequest request, SendGridMessage msg)
    {
        foreach (var attachment in request.AttachmentData)
        {
            // Convert the byte array to a Base64 string
            var base64Content = Convert.ToBase64String(attachment.Value);
            msg.AddAttachment(attachment.Key, base64Content);
        }
    }
}