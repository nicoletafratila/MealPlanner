using System.Security.Claims;
using Common.UI;
using Identity.Shared.Models;
using Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace MealPlanner.UI.Web.Pages.Identities
{
    [AllowAnonymous]
    public partial class LoginDisplay
    {
        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

        public ApplicationUserEditModel? ApplicationUser { get; private set; }

        [Inject]
        public IAuthenticationService AuthenticationService { get; set; } = default!;

        [Inject]
        public IApplicationUserService ApplicationUserService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public ILogger<LoginDisplay> Logger { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateTask;
            var user = authState.User;

            if (user.Identity?.IsAuthenticated != true)
                return;

            var username = user.Identity?.Name ?? user.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(username))
                return;

            try
            {
                ApplicationUser = await ApplicationUserService.GetEditAsync(username);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to load ApplicationUser for username '{Username}'", username);
                MessageComponent?.ShowErrorAsync(Resources.LoginDisplay.FailedToLoadUserProfile);
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                var result = await AuthenticationService.LogoutAsync();

                if (result?.Succeeded == true)
                {
                    NavigationManager.NavigateTo("/", forceLoad: true);
                }
                else
                {
                    var message = string.IsNullOrWhiteSpace(result?.Message)
                        ? Resources.LoginDisplay.LogoutFailed
                        : result!.Message!;

                    Logger.LogWarning("Logout failed. Succeeded={Succeeded}, Message={Message}, ErrorCode={ErrorCode}",
                        result?.Succeeded, result?.Message, result?.ErrorCode);

                    await MessageComponent!.ShowErrorAsync(message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error during logout");
                await MessageComponent!.ShowErrorAsync(Resources.LoginDisplay.LogoutUnexpectedError);
            }
        }
    }
}
