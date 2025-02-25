using EventBus.Contracts.Events;
using Identity.API.Models;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Identity.API.Pages.Account.ForgotPassword
{
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public class Index : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IPublishEndpoint _publishEndpoint;

        [BindProperty]
        public InputModel? Input { get; set; }

        public Index(UserManager<ApplicationUser> userManager,
           SignInManager<ApplicationUser> signInManager,
           IPublishEndpoint publishEndpoint)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _publishEndpoint = publishEndpoint;
        }

        public PageResult OnGet(string returnUrl)
        {
            Input = new InputModel { ReturnUrl = returnUrl };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid && Input != null && !string.IsNullOrWhiteSpace(Input.ReturnUrl))
                return Redirect(Input.ReturnUrl);

            var user = await _userManager.FindByEmailAsync(Input!.Email);
            if (user == null)
                return RedirectToAction(nameof(ForgotPasswordConfirmation));

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callback = Url.Action(nameof(ResetPassword), "Account", new { token, email = user.Email, returnUrl = Input.ReturnUrl }, Request.Scheme);

            await _publishEndpoint.Publish<EmailMetadata>(new
            {
                ToAddresses = new List<string>() { user.Email },
                Subject = "Reset password token",
                Body = $"Please click <a id=\"confirm-link\" href=\"{callback}\">here</a> to reset your password."
            });

            return RedirectToPage("/Account/ForgotPasswordConfirmation/Index", new { returnUrl = Input.ReturnUrl });
        }
    }
}
