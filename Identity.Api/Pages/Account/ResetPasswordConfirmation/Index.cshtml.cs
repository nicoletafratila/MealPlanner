using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Identity.Api.Pages.Account.ResetPasswordConfirmation
{
    [AllowAnonymous]
    public class Index : PageModel
    {
        //private readonly IPublishEndpoint _publishEndpoint;

        [BindProperty]
        public InputModel? Input { get; set; }

        public Index()//IPublishEndpoint publishEndpoint)
        {
           // _publishEndpoint = publishEndpoint;
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl, string email)
        {
            Input = new InputModel { ReturnUrl = returnUrl, Email = email };

            //await _publishEndpoint.Publish<EmailMetadata>(new
            //{
            //    ToAddresses = new List<string>() { Input.Email },
            //    Subject = "Reset password confirmation",
            //    Body = $"You have successfully changed your password. Access our \"Games\" page to join other players in a relaxing and fun TRIVIA game."
            //});

            return Page();
        }
    }
}
