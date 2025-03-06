using Application.Mailing;
using Domain.Common.Const;
using Domain.Common.Mailing;
using Domain.Identity;
using Domain.ViewModel;
using Infrastructure.Rendering.Models;
using Infrastructure.Rendering.ViewModels;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Mailing;

public class MailManagerService : IMailManagerService
{
    private readonly IMailService _mailService;
    private readonly IRenderingService _renderingService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public MailManagerService(
        IMailService mailService,
        IRenderingService renderingService,
        IHttpContextAccessor httpContextAccessor)
    {
        _mailService = mailService;
        _renderingService = renderingService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task SendEmailConfirmationEmailAsync(SendEmailConfirmationEmailModel model)
    {
        var renderedView = await _renderingService
            .RenderTemplateAsync(RenderingTemplates.ConfirmEmail, new ConfirmEmailVm(model.EmailConfirmationUrl)
            {
                Title = model.Title
            });

        await _mailService.SendEmailAsync(new EmailRequest(model.Email, model.Title, renderedView)
        {
            From = Constants.EmailFrom
        });

    }

    public async Task SendForgotPasswordEmailAsync(string email, string resetPasswordUrl)
    {
        var title = "Reset Password";
        var renderedView = await _renderingService
            .RenderTemplateAsync(RenderingTemplates.ForgotPassword, new ForgotPasswordVm(resetPasswordUrl)
            {
                Title = title
            });

        await _mailService.SendEmailAsync(new EmailRequest(email, title, renderedView)
        {
            From = Constants.EmailFrom
        });
    }

    public async Task SendCredientialsAndConfirmationMailUrl(ApplicationUser applicationUserDto, string emailConfirmationUrl)
    {
        var title = "Confirm your account";
        var renderedView = await _renderingService
            .RenderTemplateAsync(RenderingTemplates.CredientialsAndConfirmationMailUrl, new CredientialsAndConfirmEmailVm(emailConfirmationUrl, applicationUserDto.UserName!, applicationUserDto.PasswordHash!)
            {
                Title = title
            });

        await _mailService.SendEmailAsync(new EmailRequest(applicationUserDto.Email!, title, renderedView)
        {
            From = Constants.EmailFrom
        });
    }

    public async Task SendUpdateConfirmationMailUrl(string email)
    {
        var title = "Your account has been updated";
        var renderedView = await _renderingService
            .RenderTemplateAsync(RenderingTemplates.UpdatedAccount, new CredientialsAndConfirmEmailVm(string.Empty, email, string.Empty)
            {
                Title = title
            });

        await _mailService.SendEmailAsync(new EmailRequest(email, title, renderedView)
        {
            From = Constants.EmailFrom
        });
    }

    public async Task SendNewRegisterAccountAsync(SendEmailConfirmationEmailModel model)
    {
        var mailRequestVm = new MailRequestVm();

        var url = GetLocalhostUrl();
        var title = $"Invitation for Suite Management Portal";
        mailRequestVm.Title = title;
        mailRequestVm.UrlPath = url;
        mailRequestVm.ExecutiveEmail = model.Email;
        mailRequestVm.ExecutiveName = model.Title;
        var renderedView = await _renderingService
            .RenderTemplateAsync(RenderingTemplates.NewUserRegistered, mailRequestVm);

        await _mailService.SendEmailAsync(new EmailRequest(model.Email, title, renderedView)
        {
            From = Constants.EmailFrom
        });
    }

    public async Task SendNewRequestCreatedEmailAsync(MailRequestVm mailRequestVm)
    {
        var createOrUpdate = mailRequestVm.Id != 0 ? "Updated" : "Created";
        var url = GetLocalhostUrl();
        var title = $"Executive have {createOrUpdate} request for {mailRequestVm.EventName}";
        mailRequestVm.Title = title;
        mailRequestVm.UrlPath = url;
        var renderedView = await _renderingService
            .RenderTemplateAsync(RenderingTemplates.NewRequestCreated, mailRequestVm);

        await _mailService.SendEmailAsync(new EmailRequest(Constants.EmailAdmin, title, renderedView)
        {
            From = mailRequestVm.ExecutiveEmail
        });
    }

    public async Task SendRequestApprovedEmailAsync(MailRequestVm mailRequestVm)
    {
        var title = $"Request Approved by Admin for {mailRequestVm.EventName}";
        mailRequestVm.Title = title;
        var url = GetLocalhostUrl();
        mailRequestVm.UrlPath = url;
        var renderedView = await _renderingService
            .RenderTemplateAsync(RenderingTemplates.RequestApproved, mailRequestVm);

        await _mailService.SendEmailAsync(new EmailRequest(mailRequestVm.ExecutiveEmail!, title, renderedView)
        {
            From = Constants.EmailAdmin
        });
    }

    public async Task SendRequestFillEmailAsync(MailRequestVm mailRequestVm)
    {
        var title = $"{mailRequestVm.ActionName} Data Submitted by {mailRequestVm.ExecutiveName} for {mailRequestVm.EventName}.";
        mailRequestVm.Title = title;
        var url = GetLocalhostUrl();
        mailRequestVm.UrlPath = url;

        var renderedView = await _renderingService
            .RenderTemplateAsync(RenderingTemplates.RequestFilled, mailRequestVm);

        await _mailService.SendEmailAsync(new EmailRequest(Constants.EmailAdmin, title, renderedView)
        {
            From = mailRequestVm.ExecutiveEmail!
        });
    }

    public async Task SendRequestRejectedEmailAsync(MailRequestVm mailRequestVm)
    {
        var title = $"Request Rejected by Admin for {mailRequestVm.EventName}";
        mailRequestVm.Title = title;
        var url = GetLocalhostUrl();
        mailRequestVm.UrlPath = url;

        var renderedView = await _renderingService
            .RenderTemplateAsync(RenderingTemplates.RequestRejected, mailRequestVm);

        await _mailService.SendEmailAsync(new EmailRequest(mailRequestVm.ExecutiveEmail!, title, renderedView)
        {
            From = Constants.EmailAdmin
        });
    }

    public async Task SendNewConferenceRequestEmailAsync(MailConferenceVm mailConferenceVm)
    {
        var createOrUpdate = mailConferenceVm.Id != 0 ? "Updated" : "Created";
        var url = GetLocalhostUrl();
        var title = $"Executive have {createOrUpdate} request for conference suite";
        mailConferenceVm.Title = title;
        mailConferenceVm.UrlPath = url;
        var renderedView = await _renderingService
            .RenderTemplateAsync(RenderingTemplates.ConferenceNewRequestCreated, mailConferenceVm);

        await _mailService.SendEmailAsync(new EmailRequest(Constants.EmailAdmin, title, renderedView)
        {
            From = mailConferenceVm.ExecutiveEmail
        });
    }

    public async Task SendConferenceRequestApprovedEmailAsync(MailConferenceVm mailConferenceVm)
    {
        var title = $"Request Approved by Admin for conference suite";
        mailConferenceVm.Title = title;
        var url = GetLocalhostUrl();
        mailConferenceVm.UrlPath = url;
        var renderedView = await _renderingService
            .RenderTemplateAsync(RenderingTemplates.ConferenceRequestApproved, mailConferenceVm);

        await _mailService.SendEmailAsync(new EmailRequest(mailConferenceVm.ExecutiveEmail!, title, renderedView)
        {
            From = Constants.EmailAdmin
        });
    }

    public async Task SendConferenceRequestFillEmailAsync(MailConferenceVm mailConferenceVm)
    {
        var title = $"{mailConferenceVm.ActionName} Data Submitted by {mailConferenceVm.ExecutiveName} for Conference Request.";
        mailConferenceVm.Title = title;
        var url = GetLocalhostUrl();
        mailConferenceVm.UrlPath = url;

        var renderedView = await _renderingService
            .RenderTemplateAsync(RenderingTemplates.ConferenceRequestFilled, mailConferenceVm);

        await _mailService.SendEmailAsync(new EmailRequest(Constants.EmailAdmin, title, renderedView)
        {
            From = mailConferenceVm.ExecutiveEmail!
        });
    }

    public async Task SendConferenceRequestRejectedEmailAsync(MailConferenceVm mailConferenceVm)
    {
        var title = $"Request Rejected by Admin for conference suite";
        mailConferenceVm.Title = title;
        var url = GetLocalhostUrl();
        mailConferenceVm.UrlPath = url;

        var renderedView = await _renderingService
            .RenderTemplateAsync(RenderingTemplates.ConferenceRequestRejected, mailConferenceVm);

        await _mailService.SendEmailAsync(new EmailRequest(mailConferenceVm.ExecutiveEmail!, title, renderedView)
        {
            From = Constants.EmailAdmin
        });
    }

    public async Task SendQueryEmailAsync(MailRequestVm mailRequestVm)
    {
        var title = $"New Query Submission from Executive: {mailRequestVm.ExecutiveName}";
        mailRequestVm.Title = title;
        var url = GetLocalhostUrl();
        mailRequestVm.UrlPath = url;

        var renderedView = await _renderingService
            .RenderTemplateAsync(RenderingTemplates.ContactUs, mailRequestVm);

        await _mailService.SendEmailAsync(new EmailRequest(Constants.EmailAdmin, title, renderedView)
        {
            From = mailRequestVm.ExecutiveEmail
        });
    }

    public async Task SendAttendeeQueryEmailAsync(AttendeeEmailVm attendeeEmailVm)
    {
        var title = $"{attendeeEmailVm.Subject}";
        attendeeEmailVm.Title = title;
        var url = GetLocalhostUrl();
        attendeeEmailVm.UrlPath = url;

        var renderedView = await _renderingService
            .RenderTemplateAsync(RenderingTemplates.MailAttendeeQuery, attendeeEmailVm);

        await _mailService.SendEmailAsync(new EmailRequest(attendeeEmailVm.Email!, title, renderedView, null, null, null, null, null, null, attendeeEmailVm.Attachments)
        {
            From = Constants.EmailAdmin
        });
    }

    public async Task SendOtpMail(string email, string otp)
    {
        var title = "One Time Password (OTP) for your account.";
        var renderedView = await _renderingService
            .RenderTemplateAsync(RenderingTemplates.OtpMail, new OtpMailVm(otp)
            {
                Title = title
            });

        await _mailService.SendEmailAsync(new EmailRequest(email, title, renderedView)
        {
            From = Constants.EmailFrom
        });
    }

    public string GetLocalhostUrl()
    {
        var request = _httpContextAccessor?.HttpContext?.Request;
        var baseUrl = $"{request?.Scheme}://{request?.Host.Value}{request?.PathBase.Value}";
        return baseUrl;
    }
}