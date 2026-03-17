using Common.UI;
using Identity.Shared.Models;
using MealPlanner.UI.Web.Services.Identities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace MealPlanner.UI.Web.Pages.Identities
{
    [AllowAnonymous]
    public partial class Login
    {
        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        public LoginModel LoginModel { get; } = new();

        [Inject]
        public IAuthenticationService AuthenticationService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public IJSRuntime JS { get; set; } = default!;

        private async Task OnLoginAsync()
        {
            if (AuthenticationService is null)
            {
                await ShowErrorAsync("Authentication service is not available.");
                return;
            }

            var result = await AuthenticationService.LoginAsync(LoginModel);

            if (result is null)
            {
                await ShowErrorAsync("Login failed. Please try again.");
                return;
            }

            if (result.Succeeded)
            {
                NavigationManager.NavigateTo("/", forceLoad: true);
            }
            else
            {
                await ShowErrorAsync(result.Message ?? "Login failed. Please try again.");
            }
        }

        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await OnLoginAsync();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JS.InvokeVoidAsync("focusElement", "username");
            }
        }

        private async Task ShowErrorAsync(string message)
            => await MessageComponent!.ShowErrorAsync(message);
    }
}