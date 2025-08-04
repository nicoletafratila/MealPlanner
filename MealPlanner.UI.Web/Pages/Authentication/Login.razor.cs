using Identity.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

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
        protected AuthenticationStateProvider auth { get;set; }

        private async Task OnLoginAsync()
        {
            var result = await AuthenticationService!.LoginAsync(LoginModel);
            if (result.Succeeded)
            {
                var a =await auth.GetAuthenticationStateAsync();
                NavigationManager?.NavigateTo(NavigationManager.Uri, forceLoad: true);
            }
            else
            {
                MessageComponent?.ShowError(result!.Message!);
            }
        }
    }
}
