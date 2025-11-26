using System.Security.Claims;
using Identity.Shared.Models;
using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Services.Identities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace MealPlanner.UI.Web.Shared
{
    public partial class LoginDisplay
    {
        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

        public ApplicationUserEditModel? ApplicationUser { get; set; }

        [Inject]
        public IAuthenticationService? AuthenticationService { get; set; }

        [Inject]
        public IApplicationUserService? ApplicationUserService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateTask;
            var user = authState.User;

            if (user.Identity?.IsAuthenticated != true)
                return;

            var username = user.Identity?.Name ?? user.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(username))
                return;

            ApplicationUser = await ApplicationUserService!.GetEditAsync(username);
        }

        public async Task LogoutAsync()
        {
            var result = await AuthenticationService!.LogoutAsync();
            if (result != null && result.Succeeded)
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
