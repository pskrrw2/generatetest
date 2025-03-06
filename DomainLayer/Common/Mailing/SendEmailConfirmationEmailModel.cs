namespace Domain.Common.Mailing;

public class SendEmailConfirmationEmailModel
{
    public SendEmailConfirmationEmailModel(string email, string title, string emailConfirmationUrl)
    {
        Email = email;
        Title = title;
        EmailConfirmationUrl = emailConfirmationUrl;
    }

    public string Email { get; set; }
    public string Title { get; set; }
    public string EmailConfirmationUrl { get; set; }
}