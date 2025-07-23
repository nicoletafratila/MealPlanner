using Common.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;

namespace MealPlanner.UI.Web.Shared
{
    public partial class LoginDisplay
    {
        [Inject]
        protected NavigationManager? Navigation { get; set; }

        [Inject]
        protected IdentityApiConfig? IdentityApiConfig { get; set; }

        [Inject]
        protected MealPlannerWebConfig? MealPlannerWebConfig { get; set; }

        [Inject]
        AuthenticationStateProvider auth { get; set; }
        protected override void OnInitialized()
        {
            var b = auth.GetAuthenticationStateAsync();
            base.OnInitialized();
        }
        //private void BeginSignOut(MouseEventArgs args)
        //{
        //    string path = $"{IdentityApiConfig!.BaseUrl}/account/logout?returnUrl={MealPlannerWebConfig!.BaseUrl}";
        //    Microsoft.AspNetCore.Components.WebAssembly.Authentication.NavigationManagerExtensions.NavigateToLogout(Navigation!, path, MealPlannerWebConfig!.BaseUrl!.ToString());
        //}
    }
}
