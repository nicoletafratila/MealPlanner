using Common.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Identity.Api.Pages.Account.ChangePassword
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<IndexModel> _logger;
        //private readonly IPublishEndpoint _publishEndpoint;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<IndexModel> logger)
            //IPublishEndpoint publishEndpoint)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            //_publishEndpoint = publishEndpoint;
        }

        [BindProperty]
        public InputModel? Input { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public PageResult OnGet(string returnUrl)
        {
            Input = new InputModel { ReturnUrl = returnUrl };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input!.OldPassword, Input.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User changed their password successfully.");
            StatusMessage = "Your password has been changed.";

            //await _publishEndpoint.Publish<EmailMetadata>(new
            //{
            //    ToAddresses = new List<string>() { user.Email },
            //    Subject = "Password changed",
            //    Body = $"Hello {user.FirstName} {user.LastName}! Your password was successfully saved. Access our \"Games\" page to join other players in a relaxing and fun TRIVIA game."
            //});

            return RedirectToPage("./Index", new { Input.ReturnUrl });
        }
    }
}
