using Common.Data.Entities;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using Identity.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        AuthenticationStateProvider AuthProvider { get; set; }

        private async Task OnLoginAsync()
        {
            var result = await AuthenticationService!.LoginAsync(LoginModel);
            if (result.Succeeded)
            {
                NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
            }
            else
            {
                MessageComponent?.ShowError(result!.Message!);
            }
        }
    }
}
