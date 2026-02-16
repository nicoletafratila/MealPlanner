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

        public LoginModel LoginModel = new();

        [Inject]
        public IAuthenticationService? AuthenticationService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IJSRuntime JS { get; set; } = default!;

        private async Task OnLoginAsync()
        {
            var result = await AuthenticationService!.LoginAsync(LoginModel);
            if (result != null && result.Succeeded)
            {
                NavigationManager?.NavigateTo("/", forceLoad: true);
            }
            else
            {
                MessageComponent?.ShowError(result!.Message!);
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
    }
}
