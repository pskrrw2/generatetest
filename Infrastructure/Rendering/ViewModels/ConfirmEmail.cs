namespace Infrastructure.Rendering.ViewModels;

public class ConfirmEmailVm : EmailBaseVm
{
    public ConfirmEmailVm(string confirmEmailUrl)
    {
        ConfirmEmailUrl = confirmEmailUrl;
    }

    public string ConfirmEmailUrl { get; set; }
}