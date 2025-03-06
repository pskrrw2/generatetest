using Domain.Common.Mailing;
using Domain.Identity;
using Domain.ViewModel;

namespace Application.Mailing;

public interface IMailManagerService
{
    Task SendEmailConfirmationEmailAsync(SendEmailConfirmationEmailModel model);
    Task SendForgotPasswordEmailAsync(string email, string resetPasswordUrl);
    Task SendCredientialsAndConfirmationMailUrl(ApplicationUser applicationUserDto, string confirmationUrl);
    Task SendUpdateConfirmationMailUrl(string email);
    Task SendNewRequestCreatedEmailAsync(MailRequestVm mailRequestVm);
    Task SendRequestApprovedEmailAsync(MailRequestVm mailRequestVm);
    Task SendRequestFillEmailAsync(MailRequestVm mailRequestVm);
    Task SendRequestRejectedEmailAsync(MailRequestVm mailRequestVm);
    Task SendNewConferenceRequestEmailAsync(MailConferenceVm mailConferenceVm);
    Task SendConferenceRequestApprovedEmailAsync(MailConferenceVm mailConferenceVm);
    Task SendConferenceRequestFillEmailAsync(MailConferenceVm mailConferenceVm);
    Task SendConferenceRequestRejectedEmailAsync(MailConferenceVm mailConferenceVm);
    Task SendQueryEmailAsync(MailRequestVm mailRequestVm);
    Task SendNewRegisterAccountAsync(SendEmailConfirmationEmailModel model);
    Task SendAttendeeQueryEmailAsync(AttendeeEmailVm attendeeEmailVm);
    Task SendOtpMail(string email, string otp);
}