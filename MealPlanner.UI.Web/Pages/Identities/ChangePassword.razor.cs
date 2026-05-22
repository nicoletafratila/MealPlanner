using BlazorBootstrap;
using Common.UI;
using Identity.Shared.Models;
using MealPlanner.UI.Web.Services.Identities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.Identities
{
    [Authorize]
    public partial class ChangePassword
    {
        private List<BreadcrumbItem> _navItems = default!;
        private bool _invalidLink;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [SupplyParameterFromQuery]
        private string? UserId { get; set; }

        [SupplyParameterFromQuery]
        private string? Name { get; set; }

        public ChangePasswordModel ChangePasswordModel { get; set; } = new();

        [Inject]
        public IAuthenticationService? AuthenticationService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        protected override void OnInitialized()
        {
            _navItems =
            [
                new BreadcrumbItem { Text = Resources.ChangePassword.PageTitle }
            ];

            if (string.IsNullOrWhiteSpace(UserId))
            {
                _invalidLink = true;
                return;
            }

            ChangePasswordModel.UserId = UserId;
        }

        private async Task OnSubmitAsync()
        {
            var result = await AuthenticationService!.ChangePasswordAsync(ChangePasswordModel);
            if (result is null)
            {
                await MessageComponent!.ShowErrorAsync(Resources.ChangePassword.ChangePasswordFailed);
                return;
            }

            if (!result.Succeeded)
            {
                await MessageComponent!.ShowErrorAsync(result.Message!);
                return;
            }

            await MessageComponent!.ShowInfoAsync(Resources.ChangePassword.ChangePasswordSucceeded);
            NavigateBack();
        }

        private void NavigateBack()
        {
            var url = string.IsNullOrWhiteSpace(Name)
                ? "identities/userprofile"
                : $"identities/userprofile/{Name}";
            NavigationManager?.NavigateTo(url);
        }
    }
}
