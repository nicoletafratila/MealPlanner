using Common.UI;
using Identity.Services.Http;
using Identity.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.Identities
{
    [AllowAnonymous]
    public partial class ForgotPassword
    {
        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        public ForgotPasswordModel ForgotPasswordModel { get; } = new();

        [Inject]
        public IAuthenticationService AuthenticationService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        private async Task OnSubmitAsync()
        {
            var result = await AuthenticationService.ForgotPasswordAsync(ForgotPasswordModel);

            if (result is null)
            {
                await MessageComponent!.ShowErrorAsync(Resources.ForgotPassword.SubmitFailed);
                return;
            }

            if (result.Succeeded)
            {
                await MessageComponent!.ShowInfoAsync(result.Message ?? Resources.ForgotPassword.SubmitFailed);
            }
            else
            {
                await MessageComponent!.ShowErrorAsync(result.Message ?? Resources.ForgotPassword.SubmitFailed);
            }
        }

        private void NavigateToLogin()
        {
            NavigationManager.NavigateTo("identities/login");
        }
    }
}
