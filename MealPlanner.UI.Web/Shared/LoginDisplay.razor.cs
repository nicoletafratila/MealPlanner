using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Shared
{
    public partial class LoginDisplay
    {
        [Inject]
        public NavigationManager? Navigation { get; set; }

        //private void BeginSignOut(MouseEventArgs args)
        //{
        //    string path = $"{IdentityApiConfig!.BaseUrl}/account/logout?returnUrl={MealPlannerWebConfig!.BaseUrl}";
        //    Microsoft.AspNetCore.Components.WebAssembly.Authentication.NavigationManagerExtensions.NavigateToLogout(Navigation!, path, MealPlannerWebConfig!.BaseUrl!.ToString());
        //}
    }
}
