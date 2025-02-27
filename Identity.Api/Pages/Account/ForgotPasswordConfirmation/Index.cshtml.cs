using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Identity.Api.Pages.Account.ForgotPasswordConfirmation
{
    [AllowAnonymous]
    public class Index : PageModel
    {
        [BindProperty]
        public InputModel? Input { get; set; }

        public PageResult OnGet(string returnUrl)
        {
            Input = new InputModel { ReturnUrl = returnUrl };
            return Page();
        }
    }
}
