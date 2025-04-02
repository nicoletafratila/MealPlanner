using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Common.Api
{
    public class AuthHandler : AuthorizationMessageHandler
    {
        public AuthHandler(
            IAccessTokenProvider provider, 
            NavigationManager navigation, 
            MealPlannerApiConfig mealPlannerApiConfig, 
            RecipeBookApiConfig recipeBookApiConfig,
            IdentityApiConfig identityApiConfig) 
            : base(provider, navigation)
        {
            ConfigureHandler(
                new List<string> { mealPlannerApiConfig.BaseUrl!.AbsoluteUri, recipeBookApiConfig.BaseUrl!.AbsoluteUri, identityApiConfig.BaseUrl!.AbsoluteUri },
                new List<string> { "profile", "MealPlanner.Api.full", "email", "role", "IdentityServerApi" });
        }
    }
}
