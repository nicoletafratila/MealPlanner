using Identity.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.Authentication
{
    [AllowAnonymous]
    public partial class Login
    {
        public LoginModel? Credential { get; set; }

        [Inject]
        public IAuthenticationService? AuthenticationService { get; set; }

        protected override void OnInitialized()
        {
            Credential = new LoginModel();
        }

        private async Task AuthenticateAsync()
        {
            var response = await AuthenticationService!.LoginAsync(Credential!);
            //if (!string.IsNullOrWhiteSpace(response))
            //{
            //    MessageComponent?.ShowError(response);
            //}
            //else
            //{
            //    MessageComponent?.ShowInfo("Data has been saved successfully");
            //    NavigateToOverview();
            //}
        }
    }
}
