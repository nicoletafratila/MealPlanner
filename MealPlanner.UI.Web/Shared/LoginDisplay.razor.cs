using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Services.Identities;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Shared
{
    public partial class LoginDisplay
    {
        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Inject]
        public IAuthenticationService? AuthenticationService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        public async Task LogoutAsync()
        {
            var result = await AuthenticationService!.LogoutAsync();
            if (result.Succeeded)
            {
                NavigationManager?.NavigateTo("/", forceLoad: true);
            }
            else
            {
                MessageComponent?.ShowError(result!.Message!);
            }
        }
    }
}
