using EventBus.Contracts.Events;
using Identity.API.Models;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Identity.API.Pages.Account.Register
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<IndexModel> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            ILogger<IndexModel> logger,
            IPublishEndpoint publishEndpoint)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        [BindProperty]
        public InputModel? Input { get; set; }

        public string? ReturnUrl { get; set; }

        public Task OnGetAsync(string returnUrl)
        {
            ReturnUrl = returnUrl;
            return Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input!.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.IsActive = true;
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    if (!await _userManager.IsInRoleAsync(user, "member"))
                    {
                        var roleResult = await _userManager.AddToRoleAsync(user, "member");
                        if (!roleResult.Succeeded)
                        {
                            throw new Exception(roleResult.Errors.First().Description);
                        }
                        result = await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, "member"));

                        if (result.Succeeded)
                        {
                            await _publishEndpoint.Publish<EmailMetadata>(new
                            {
                                ToAddresses = new List<string>() { Input!.Email! },
                                Subject = "Welcome!",
                                Body = $"Hello {user.FirstName} {user.LastName}! Thank you for your registration on Congnizant's Trivia game. Access our \"Games\" page to join other players in a relaxing and fun TRIVIA game."
                            });

                            await _publishEndpoint.Publish<UserRegistered>(new
                            {
                                UserId = user.Id,
                                NickName = user.UserName
                            });

                            _logger.LogInformation("User created a new account with password.");
                            return RedirectToPage("/Account/RegisterConfirmation/Index", new { email = Input!.Email!, DisplayConfirmAccountLink = true, returnUrl = returnUrl });
                        }
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
