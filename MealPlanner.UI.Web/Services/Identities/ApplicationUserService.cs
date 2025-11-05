using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using Identity.Shared.Models;

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
    }
}
