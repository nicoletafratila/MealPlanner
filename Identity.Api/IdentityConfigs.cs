using System.Security.Claims;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Identity.Api
{
    public static class IdentityConfigs
    {
        private const string CLIENTID = "mealplanner_client";

        public static IEnumerable<Client> GetClients() =>
            new List<Client>
            {
                new Client
                {
                    ClientId = CLIENTID,
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets = { new Secret(Common.Constants.MealPlanner.SigningKey.Sha256()) },
                    AllowedScopes = { Common.Constants.MealPlanner.ApiScope, IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile, "roles" }
                }
            };

        public static IEnumerable<ApiScope> GetApiScopes() =>
            new List<ApiScope>
            {
                new ApiScope(Common.Constants.MealPlanner.ApiScope, "MealPlanner API"/*, new[] { ClaimTypes.Role }*/)
            };

        public static IEnumerable<ApiResource> GetApiResources() =>
            new List<ApiResource>
            {
                new ApiResource(Common.Constants.MealPlanner.ApiScope, "MealPlanner API")
                {
                    Scopes = { Common.Constants.MealPlanner.ApiScope },
                    //UserClaims = { ClaimTypes.Role }
                }
            };

        public static IEnumerable<IdentityResource> GetIdentityResources() =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                // IdentityResource("roles", new[] { ClaimTypes.Role })
            };
    }
}