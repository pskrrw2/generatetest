namespace Infrastructure.Rendering.ViewModels;

public class CredientialsAndConfirmEmailVm : EmailBaseVm
{
    public CredientialsAndConfirmEmailVm(string confirmEmailUrl, string username, string generatedPassword)
    {
        ConfirmEmailUrl = confirmEmailUrl;
        Username = username;
        GeneratedPassword = generatedPassword;
    }

    public string ConfirmEmailUrl { get; set; }
    public string Username { get; set; }
    public string GeneratedPassword { get; set; }
}