using System.Security.Claims;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Identity.Api
{
    public static class IdentityConfigs
    {
        private const string ClientId = "mealplanner_client";
        private const string ApiName = Common.Constants.MealPlanner.ApiScope;
        private const string ApiDisplayName = "MealPlanner API";
        private static readonly string SigningKey = Common.Constants.MealPlanner.SigningKey;

        public static IEnumerable<Client> GetClients() =>
            [
                new Client
                {
                    ClientId = ClientId,
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets =
                    {
                        new Secret(SigningKey.Sha256())
                    },
                    AllowedScopes =
                    {
                        ApiName,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "roles"
                    }
                }
            ];

        public static IEnumerable<ApiScope> GetApiScopes() =>
            [
                new ApiScope(ApiName, ApiDisplayName)
                {
                    UserClaims =
                    {
                        ClaimTypes.Role,
                        ClaimTypes.Name
                    }
                }
            ];

        public static IEnumerable<ApiResource> GetApiResources() =>
            [
                new ApiResource(ApiName, ApiDisplayName)
                {
                    Scopes = { ApiName },
                    UserClaims =
                    {
                        ClaimTypes.Role,
                        ClaimTypes.Name
                    }
                }
            ];

        public static IEnumerable<IdentityResource> GetIdentityResources() =>
            [
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new("roles", [ClaimTypes.Role])
            ];
    }
}