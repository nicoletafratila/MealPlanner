using Common.UI;
using Identity.Services.Core;
using Identity.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.Identities
{
    [AllowAnonymous]
    public partial class ResetPassword
    {
        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        public ResetPasswordModel ResetPasswordModel { get; } = new();

        [Inject]
        public IAuthenticationService AuthenticationService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [SupplyParameterFromQuery(Name = "userId")]
        private string? UserId { get; set; }

        [SupplyParameterFromQuery(Name = "token")]
        private string? Token { get; set; }

        private bool _invalidLink;

        protected override void OnInitialized()
        {
            if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(Token))
            {
                _invalidLink = true;
                return;
            }

            ResetPasswordModel.UserId = UserId;
            ResetPasswordModel.Token = Token;
        }

        private async Task OnSubmitAsync()
        {
            var result = await AuthenticationService.ResetPasswordAsync(ResetPasswordModel);

            if (result is null)
            {
                await MessageComponent!.ShowErrorAsync(Resources.ResetPassword.SubmitFailed);
                return;
            }

            if (result.Succeeded)
            {
                await MessageComponent!.ShowInfoAsync(result.Message!);
                NavigationManager.NavigateTo("identities/login", forceLoad: true);
            }
            else
            {
                await MessageComponent!.ShowErrorAsync(result.Message ?? Resources.ResetPassword.SubmitFailed);
            }
        }

        private void NavigateToLogin()
        {
            NavigationManager.NavigateTo("identities/login", forceLoad: true);
        }
    }
}
