using System.Text;
using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using Common.Models;
using Duende.IdentityModel;
using Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace MealPlanner.UI.Web.Services
{
    public class AuthenticationService(HttpClient httpClient, TokenProvider tokenProvider) : IAuthenticationService
    {
        private readonly IApiConfig _identityApiConfig = ServiceLocator.Current.GetInstance<IdentityApiConfig>();

        public async Task<CommandResponse> LoginAsync(LoginModel model)
        {
            //var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            //var response = await _httpClient.PostAsync($"{_identityApiConfig?.Controllers![IdentityControllers.Authentication]}/login", modelJson);
            //var result = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            //{
            //    Message = string.Empty
            //});
            //return result;


            //var response = await _http.PostAsJsonAsync("/api/auth/login", new { username, password });
            //if (response.IsSuccessStatusCode)
            //{
            //    var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            //    await _tokenProvider.StoreTokensAsync(result);
            //    return LoginResult.Success();
            //}
            //else
            //{
            //    var error = await response.Content.ReadAsStringAsync();
            //    return LoginResult.Failed(error);
            //}

            //var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            //var response = await _httpClient.PostAsync($"{_identityApiConfig?.Controllers![IdentityControllers.Authentication]}/login", modelJson);
            //if (response.IsSuccessStatusCode)
            //{
            //    var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            //    await _tokenProvider.StoreTokensAsync(result);
            //    return LoginResult.Success();
            //}
            //else
            //{
            //    var error = await response.Content.ReadAsStringAsync();
            //    return LoginResult.Failed(error);
            //}

            var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{_identityApiConfig?.Controllers![IdentityControllers.Authentication]}/login", modelJson);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginCommandResponse>();
                if (loginResponse != null && loginResponse.Succeeded && !string.IsNullOrEmpty(loginResponse.JwtBearer))
                {
                    await tokenProvider.StoreTokensAsync(loginResponse.JwtBearer);
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
