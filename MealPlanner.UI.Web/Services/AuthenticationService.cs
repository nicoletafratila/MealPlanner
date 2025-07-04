using System.Text;
using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using Identity.Shared.Models;
using Newtonsoft.Json;

namespace MealPlanner.UI.Web.Services
{
    public class AuthenticationService(HttpClient httpClient) : IAuthenticationService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IApiConfig _identityApiConfig = ServiceLocator.Current.GetInstance<IdentityApiConfig>();

        public async Task<string?> LoginAsync(LoginModel model)
        {
            var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_identityApiConfig?.Controllers![IdentityControllers.Authentication]}/login", modelJson);
            var result = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }

        public Task<string?> RegisterAsync(RegistrationModel model)
        {
            throw new NotImplementedException();
        }
    }
}
