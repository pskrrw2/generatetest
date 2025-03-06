namespace Infrastructure.Rendering.ViewModels;

public class ForgotPasswordVm : EmailBaseVm
{
    public ForgotPasswordVm(string resetPasswordUrl)
    {
        ResetPasswordUrl = resetPasswordUrl;
    }

    public string ResetPasswordUrl { get; set; }
}