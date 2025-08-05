using Common.Api;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Shared
{
    public partial class LoginDisplay
    {
        [Inject]
        public NavigationManager? Navigation { get; set; }

        [Inject]
        public IdentityApiConfig? IdentityApiConfig { get; set; }

        [Inject]
        public MealPlannerWebConfig? MealPlannerWebConfig { get; set; }

        [Inject]
        protected AuthenticationStateProvider? AuthenticationStateProvider { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var state = await AuthenticationStateProvider!.GetAuthenticationStateAsync();
            var user = state.User;
        }

        //private void BeginSignOut(MouseEventArgs args)
        //{
        //    string path = $"{IdentityApiConfig!.BaseUrl}/account/logout?returnUrl={MealPlannerWebConfig!.BaseUrl}";
        //    Microsoft.AspNetCore.Components.WebAssembly.Authentication.NavigationManagerExtensions.NavigateToLogout(Navigation!, path, MealPlannerWebConfig!.BaseUrl!.ToString());
        //}
    }
}
