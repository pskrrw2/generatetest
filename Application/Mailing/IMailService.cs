namespace Application.Mailing;

public interface IMailService 
{
    Task SendEmailAsync(EmailRequest emailRequest);
}