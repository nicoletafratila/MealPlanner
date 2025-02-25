using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Common.Api
{
    public class AuthHandler : AuthorizationMessageHandler
    {
        public AuthHandler(NavigationManager navigation, MealPlannerApiConfig mealPlannerApiConfig, RecipeBookApiConfig recipeBookApiConfig, IdentityServerOptions identityServerOptions) : base(provider, navigation)
        {
            ConfigureHandler(new[] { mealPlannerApiConfig.BaseUrl!.AbsoluteUri, identityServerOptions.Authority },
                new[] { "profile", "MealPlanner.Api", "email", "role", "IdentityServerApi" });
            ConfigureHandler(new[] { recipeBookApiConfig.BaseUrl!.AbsoluteUri, identityServerOptions.Authority },
                new[] { "profile", "RecipeBook.Api", "email", "role", "IdentityServerApi" });
        }
    }
}
