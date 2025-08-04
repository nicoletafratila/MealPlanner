using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.Authentication
{
    public partial class RedirectToLogin
    {
        [Inject]
        protected NavigationManager? Navigation { get; set; }

        [Inject]
        public AuthenticationStateProvider? AuthStateProvider { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider!.GetAuthenticationStateAsync();
            if (authState?.User?.Identity == null || !authState.User.Identity.IsAuthenticated)
            {
                var returnUrl = Navigation!.ToBaseRelativePath(Navigation.Uri);
                if (string.IsNullOrEmpty(returnUrl))
                    Navigation.NavigateTo("/authentication/login", true);
                else
                    Navigation.NavigateTo("/authentication/login?returnUrl=" + returnUrl, true);
            }
        }
    }
}
