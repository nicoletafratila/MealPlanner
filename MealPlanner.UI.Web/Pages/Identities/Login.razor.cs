using Common.UI;
using Identity.Services.Http;
using Identity.Shared.Models;
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

        [SupplyParameterFromQuery]
        private bool? EmailConfirmed { get; set; }

        [SupplyParameterFromQuery]
        private bool? EmailConfirmationFailed { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (EmailConfirmed == true)
            {
                await MessageComponent!.ShowInfoAsync(Resources.Login.EmailConfirmedMessage);
            }
            else if (EmailConfirmationFailed == true)
            {
                await MessageComponent!.ShowErrorAsync(Resources.Login.EmailConfirmationFailedMessage);
            }
        }

        private async Task OnLoginAsync()
        {
            if (AuthenticationService is null)
            {
                await ShowErrorAsync(Resources.Login.AuthServiceUnavailable);
                return;
            }

            var result = await AuthenticationService.LoginAsync(LoginModel);

            if (result is null)
            {
                await ShowErrorAsync(Resources.Login.LoginFailed);
                return;
            }

            if (result.Succeeded)
            {
                NavigationManager.NavigateTo("/", forceLoad: true);
            }
            else
            {
                await ShowErrorAsync(result.Message ?? Resources.Login.LoginFailed);
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

        private void NavigateToForgotPassword()
        {
            NavigationManager.NavigateTo("identities/forgot-password");
        }

        private async Task ShowErrorAsync(string message)
            => await MessageComponent!.ShowErrorAsync(message);
    }
}