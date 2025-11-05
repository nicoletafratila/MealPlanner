using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.Identities
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
                    Navigation.NavigateTo("/identities/login", true);
                else
                    Navigation.NavigateTo("/identities/login?returnUrl=" + returnUrl, true);
            }
        }
    }
}
