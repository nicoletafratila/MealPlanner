using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace MealPlanner.UI.Web.Pages.Identities
{
    public partial class RedirectToLogin
    {
        [Inject]
        protected NavigationManager Navigation { get; set; } = default!;

        [CascadingParameter]
        private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (AuthenticationStateTask is null)
                return;

            var authState = await AuthenticationStateTask;

            if (authState?.User?.Identity == null || !authState.User.Identity.IsAuthenticated)
            {
                var returnUrl = Navigation.ToBaseRelativePath(Navigation.Uri);

                if (string.IsNullOrEmpty(returnUrl))
                {
                    Navigation.NavigateTo("/identities/login", forceLoad: true);
                }
                else
                {
                    var encoded = Uri.EscapeDataString(returnUrl);
                    Navigation.NavigateTo($"/identities/login?returnUrl={encoded}", forceLoad: true);
                }
            }
        }
    }
}