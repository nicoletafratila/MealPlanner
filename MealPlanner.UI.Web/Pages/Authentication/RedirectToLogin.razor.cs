using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace MealPlanner.UI.Web.Pages.Authentication
{
    public partial class RedirectToLogin
    {
        [Inject]
        protected NavigationManager? Navigation { get; set; }

        //protected override void OnInitialized()
        //{
        //    Navigation!.NavigateTo($"authentication/login?returnUrl={Uri.EscapeDataString(Navigation.Uri)}");
        //}

        //[CascadingParameter]
        //private Task<AuthenticationState>? AuthState { get; set; } = null;

        [Inject]
        public AuthenticationStateProvider? AuthStateProvider { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider!.GetAuthenticationStateAsync();
            //var authState = await AuthState!;
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
