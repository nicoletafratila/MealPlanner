using Identity.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace MealPlanner.UI.Web.Pages.Authentication
{
    [AllowAnonymous]
    public partial class Login
    {
        public LoginModel LoginModel = new();

        [Inject]
        public IAuthenticationService? AuthenticationService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [CascadingParameter(Name = "MessageComponent")]
        protected IMessageComponent? MessageComponent { get; set; }

        [Inject] 
        AuthenticationStateProvider stateProvider { get; set; }

        private async Task OnLoginAsync()
        {
            var result = await AuthenticationService!.LoginAsync(LoginModel);
            var authState = await stateProvider.GetAuthenticationStateAsync();
            var isAuthenticated = authState.User?.Identity?.IsAuthenticated ?? false;
            var userName = authState.User?.Identity?.Name;

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
