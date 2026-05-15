using Common.UI;
using Identity.Shared.Models;
using MealPlanner.UI.Web.Services.Identities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.Identities
{
    [AllowAnonymous]
    public partial class Register
    {
        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        public RegistrationModel RegistrationModel { get; } = new();

        [Inject]
        public IAuthenticationService AuthenticationService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        private async Task OnRegisterAsync()
        {
            var result = await AuthenticationService.RegisterAsync(RegistrationModel);

            if (result is null)
            {
                await MessageComponent!.ShowErrorAsync(Resources.Register.RegisterFailed);
                return;
            }

            if (result.Succeeded)
            {
                await MessageComponent!.ShowInfoAsync(result.Message ?? Resources.Register.RegisterSucceeded);
                NavigationManager.NavigateTo("identities/login");
            }
            else
            {
                await MessageComponent!.ShowErrorAsync(result.Message ?? Resources.Register.RegisterFailed);
            }
        }

        private void NavigateToLogin()
        {
            NavigationManager.NavigateTo("identities/login");
        }
    }
}
