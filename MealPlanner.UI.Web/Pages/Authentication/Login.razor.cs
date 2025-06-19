using Common.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

namespace MealPlanner.UI.Web.Pages.Authentication
{
    [AllowAnonymous]
    public partial class Login
    {
        private Credential Credential = new Credential();

        [Inject]
        public SignInManager<ApplicationUser> SignInManager { get; set; }

        private async Task AuthenticateAsync()
        {
            var result = await SignInManager.PasswordSignInAsync(Credential.Username, Credential.Password, Credential.RememberLogin, lockoutOnFailure: true);
            if (result.Succeeded)
            {
            }
        }
    }

    public class Credential
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public bool RememberLogin { get; set; } = false;
    }
}
