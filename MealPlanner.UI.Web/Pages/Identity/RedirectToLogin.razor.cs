using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.Identity
{
    public partial class RedirectToLogin
    {
        [Inject]
        protected NavigationManager? Navigation { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateTask!;
            if (authState?.User?.Identity == null || !authState.User.Identity.IsAuthenticated)
            {
                var returnUrl = Navigation!.ToBaseRelativePath(Navigation.Uri);
                if (string.IsNullOrEmpty(returnUrl))
                    Navigation.NavigateTo("/identity/login", true);
                else
                    Navigation.NavigateTo("/identity/login?returnUrl=" + returnUrl, true);
            }
        }
    }
}
