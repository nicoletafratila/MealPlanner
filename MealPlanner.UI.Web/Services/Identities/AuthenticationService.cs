using System.Text;
using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using Common.Models;
using Identity.Shared.Models;
using Newtonsoft.Json;

namespace MealPlanner.UI.Web.Services.Identities
{
    public class AuthenticationService(HttpClient httpClient, TokenProvider tokenProvider) : IAuthenticationService
    {
        private readonly IApiConfig _identityApiConfig = ServiceLocator.Current.GetInstance<IdentityApiConfig>();

        public async Task<CommandResponse?> LoginAsync(LoginModel model)
        {
            var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{_identityApiConfig?.Controllers![IdentityControllers.Authentication]}/login", modelJson);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginCommandResponse>();
                if (loginResponse != null && loginResponse.Succeeded)
                {
                    await tokenProvider.SetTokenAsync(loginResponse!.JwtBearer!);
                    return loginResponse;
                }
                else
                {
                    return CommandResponse.Failed(loginResponse?.Message ?? "Authentication failed.");
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return CommandResponse.Failed(error);
            }
        }

        public async Task<CommandResponse?> LogoutAsync()
        {
            var response = await httpClient.PostAsync($"{_identityApiConfig?.Controllers![IdentityControllers.Authentication]}/logout", new StringContent(string.Empty));
            if (response.IsSuccessStatusCode)
            {
                var logoutResponse = await response.Content.ReadFromJsonAsync<CommandResponse>();
                if (logoutResponse != null && logoutResponse.Succeeded)
                {
                    await tokenProvider.RemoveTokenAsync();
                    return logoutResponse;
                }
                else
                {
                    return CommandResponse.Failed(logoutResponse?.Message ?? "Logout failed.");
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return CommandResponse.Failed(error);
            }
        }

        public Task<CommandResponse?> RegisterAsync(RegistrationModel model)
        {
            throw new NotImplementedException();
        }
    }
}
