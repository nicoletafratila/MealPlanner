using Common.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Identity.Api.Pages.Account.ResetPassword
{
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public class Index : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public Index(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel? Input { get; set; }

        public PageResult OnGet(string token, string email, string returnUrl)
        {
            Input = new InputModel { Token = token, Email = email, ReturnUrl = returnUrl };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Redirect(Input!.ReturnUrl!);

            var user = await _userManager.FindByEmailAsync(Input!.Email!);
            if (user == null)
                RedirectToAction(nameof(ResetPasswordConfirmation));

            var resetPassResult = await _userManager.ResetPasswordAsync(user!, Input.Token, Input.NewPassword);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }

                return Redirect("~/");
            }

            return RedirectToPage("/Account/ResetPasswordConfirmation/Index", new { returnUrl = Input.ReturnUrl, email = Input.Email });
        }
    }
}
