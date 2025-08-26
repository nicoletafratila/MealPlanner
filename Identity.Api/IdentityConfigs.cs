using Duende.IdentityServer.Models;

namespace Identity.Api
{
    public static class IdentityConfigs
    {
        public static IEnumerable<Client> GetClients() =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "mealplanner-client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets = { new Secret(Common.Constants.MealPlanner.SigningKey.Sha256()) },
                    AllowedScopes = { "mealplanner_api", "openid", "profile" }
                }
            };

        public static IEnumerable<ApiScope> GetApiScopes() =>
            new List<ApiScope>
            {
                new ApiScope("mealplanner_api", "MealPlanner API")
            };

        public static IEnumerable<ApiResource> GetApiResources() =>
            new List<ApiResource>
            {
                new ApiResource("mealplanner_api", "MealPlanner API")
                {
                    Scopes = { "mealplanner_api" }
                }
            };

        public static IEnumerable<IdentityResource> GetIdentityResources() =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
    }
}
