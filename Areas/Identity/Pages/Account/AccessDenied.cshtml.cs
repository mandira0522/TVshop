using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TvShop.Areas.Identity.Pages.Account
{
    public class AccessDeniedModel : PageModel
    {
        public AccessDeniedModel()
        {
            ReturnUrl = "/";
        }

        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        public void OnGet(string returnUrl = "/")
        {
            ReturnUrl = returnUrl;
        }
    }
}
