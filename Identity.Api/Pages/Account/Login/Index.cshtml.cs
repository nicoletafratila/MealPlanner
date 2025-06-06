using Common.Data.Entities;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Identity.Api.Pages.Account.Login
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class Index : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;
        private readonly IAuthenticationSchemeProvider _schemeProvider;

        public ViewModel? View { get; set; }

        [BindProperty]
        public InputModel? Input { get; set; }

        public Index(
            IIdentityServerInteractionService interaction,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _schemeProvider = schemeProvider;
            _events = events;
        }

        public async Task<IActionResult> OnGet(string returnUrl)
        {
            await BuildModelAsync(returnUrl);

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(Input!.ReturnUrl);

            // the user clicked the "cancel" button
            if (Input.Button != "login")
            {
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    if (context.IsNativeClient())
                    {
                        // The client is native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage(Input!.ReturnUrl);
                    }

                    return Redirect(Input!.ReturnUrl!);
                }
                else
                {
                    // since we don't have a valid context, then we just go back to the home page
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(Input.Username);
                if (user != null)
                {
                    if (!user.IsActive)
                    {
                        await _events.RaiseAsync(new UserLoginFailureEvent(Input.Username, "User is not active", clientId: context?.Client.ClientId));
                        ModelState.AddModelError("UserIsNotActive", LoginOptions.UserIsNotActiveErrorMessage);
                    }
                    else
                    {
                        var result = await _signInManager.PasswordSignInAsync(Input.Username, Input.Password, Input.RememberLogin, lockoutOnFailure: true);
                        if (result.Succeeded)
                        {
                            await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName, clientId: context?.Client.ClientId));

                            if (context != null)
                            {
                                if (context.IsNativeClient())
                                {
                                    // The client is native, so this change in how to
                                    // return the response is for better UX for the end user.
                                    return this.LoadingPage(Input.ReturnUrl);
                                }

                                // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                                return Redirect(Input!.ReturnUrl!);
                            }

                            // request for a local page
                            if (Url.IsLocalUrl(Input.ReturnUrl))
                            {
                                return Redirect(Input.ReturnUrl);
                            }
                            else if (string.IsNullOrEmpty(Input.ReturnUrl))
                            {
                                return Redirect("~/");
                            }
                            else
                            {
                                // user might have clicked on a malicious link - should be logged
                                throw new Exception("invalid return URL");
                            }
                        }

                        await _events.RaiseAsync(new UserLoginFailureEvent(Input.Username, "Invalid credentials", clientId: context?.Client.ClientId));
                        ModelState.AddModelError("InvalidCredentials", LoginOptions.InvalidCredentialsErrorMessage);
                    }
                }
                else
                {
                    await _events.RaiseAsync(new UserLoginFailureEvent(Input.Username, "Invalid credentials", clientId: context?.Client.ClientId));
                    ModelState.AddModelError("InvalidCredentials", LoginOptions.InvalidCredentialsErrorMessage);
                }
            }

            // something went wrong, show form with error
            await BuildModelAsync(Input!.ReturnUrl);
            return Page();
        }

        private async Task BuildModelAsync(string? returnUrl)
        {
            Input = new InputModel
            {
                ReturnUrl = returnUrl
            };

            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == Duende.IdentityServer.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                View = new ViewModel
                {
                    EnableLocalLogin = local,
                };

                Input.Username = context?.LoginHint;

                return;
            }

            var allowLocal = true;
            var client = context?.Client;
            if (client != null)
            {
                allowLocal = client.EnableLocalLogin;
            }

            View = new ViewModel
            {
                AllowRememberLogin = LoginOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && LoginOptions.AllowLocalLogin,
            };
        }
    }
}