using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using Identity.Shared.Models;

namespace MealPlanner.UI.Web.Services.Identity
{
    public class ApplicationUserService(HttpClient httpClient, TokenProvider tokenProvider) : IApplicationUserService
    {
        private readonly IApiConfig _identityApiConfig = ServiceLocator.Current.GetInstance<IdentityApiConfig>();

        public async Task<ApplicationUserEditModel?> GetEditAsync(string name)
        {
            var encodedName = Uri.EscapeDataString(name);
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            //var b = await httpClient.GetFromJsonAsync<RecipeEditModel?>($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.Recipe]}/edit/{100}");

            var a = await httpClient.GetAsync($"{_identityApiConfig?.Controllers![IdentityControllers.ApplicationUser]}/edit?username={encodedName}");
            return new ApplicationUserEditModel();
        }
    }
}
