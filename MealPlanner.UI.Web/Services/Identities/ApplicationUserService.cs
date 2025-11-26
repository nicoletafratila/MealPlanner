using System.Text;
using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using Common.Models;
using Identity.Shared.Models;
using Newtonsoft.Json;

namespace MealPlanner.UI.Web.Services.Identities
{
    public class ApplicationUserService(HttpClient httpClient, TokenProvider tokenProvider) : IApplicationUserService
    {
        private readonly IApiConfig _identityApiConfig = ServiceLocator.Current.GetInstance<IdentityApiConfig>();

        public async Task<ApplicationUserEditModel?> GetEditAsync(string name)
        {
            var encodedName = Uri.EscapeDataString(name);
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            return await httpClient.GetFromJsonAsync<ApplicationUserEditModel?>($"{_identityApiConfig?.Controllers![IdentityControllers.ApplicationUser]}/edit?username={encodedName}");
        }

        public async Task<CommandResponse?> UpdateAsync(ApplicationUserEditModel model)
        {
            var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var response = await httpClient.PutAsync(_identityApiConfig?.Controllers![IdentityControllers.ApplicationUser], modelJson);
            return JsonConvert.DeserializeObject<CommandResponse>(await response.Content.ReadAsStringAsync());
        }
    }
}
