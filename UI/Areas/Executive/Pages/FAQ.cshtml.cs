using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Areas.Executive.Pages;

[Authorize]
public class FAQModel : PageModel
{
    public void OnGet()
    {
    }
}
