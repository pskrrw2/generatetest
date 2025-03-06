namespace Infrastructure.Rendering.ViewModels;

public class OtpMailVm : EmailBaseVm
{
    public OtpMailVm(string otpCode)
    {
        OtpCode = otpCode;
    }

    public string OtpCode { get; set; }
}