using Identity.Shared.Models;
using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Services.Identities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.Identities
{
    [AllowAnonymous]
    public partial class Login
    {
        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        public LoginModel LoginModel = new();

        [Inject]
        public IAuthenticationService? AuthenticationService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        private async Task OnLoginAsync()
        {
            var result = await AuthenticationService!.LoginAsync(LoginModel);
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
