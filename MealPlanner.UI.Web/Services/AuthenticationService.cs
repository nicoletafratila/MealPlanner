using System.Text;
using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using Common.Models;
using Identity.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;

namespace MealPlanner.UI.Web.Services
{
    public class AuthenticationService(HttpClient httpClient, AuthenticationStateProvider authStateProvider) : IAuthenticationService
    {
        private readonly IApiConfig _identityApiConfig = ServiceLocator.Current.GetInstance<IdentityApiConfig>();

        public async Task<CommandResponse> LoginAsync(LoginModel model)
        {
            var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{_identityApiConfig?.Controllers![IdentityControllers.Authentication]}/login", modelJson);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginCommandResponse>();
                if (loginResponse != null && loginResponse.Succeeded && !string.IsNullOrEmpty(loginResponse.JwtBearer))
                {
                    var jwtProvider = (JwtAuthenticationStateProvider)authStateProvider;
                    await jwtProvider.MarkUserAsAuthenticated(loginResponse.JwtBearer);
                    return CommandResponse.Success();
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

        public Task<CommandResponse> RegisterAsync(RegistrationModel model)
        {
            throw new NotImplementedException();
        }
    }
}
