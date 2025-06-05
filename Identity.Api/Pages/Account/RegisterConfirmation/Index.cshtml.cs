using System.Text;
using Common.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Identity.Api.Pages.Account.RegisterConfirmation
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        //private readonly IPublishEndpoint _publishEndpoint;

        public IndexModel(UserManager<ApplicationUser> userManager)
            //IPublishEndpoint publishEndpoint)
        {
            _userManager = userManager;
            //_publishEndpoint = publishEndpoint;
        }

        public string? Email { get; set; }

        public bool DisplayConfirmAccountLink { get; set; }

        public string? EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, bool displayConfirmAccountLink, string returnUrl)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            Email = email;
            DisplayConfirmAccountLink = displayConfirmAccountLink;

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            EmailConfirmationUrl = Url.Page(
                "/Account/ConfirmEmail/Index",
                pageHandler: null,
                values: new { userId, code, returnUrl },
                protocol: Request.Scheme);

            //await _publishEndpoint.Publish<EmailMetadata>(new
            //{
            //    ToAddresses = new List<string>() { Email },
            //    Subject = "Confirm registration",
            //    Body = $"Please click <a id=\"confirm-link\" href=\"{EmailConfirmationUrl}\">here</a> to confirm your email address."
            //});

            return Page();
        }
    }
}
