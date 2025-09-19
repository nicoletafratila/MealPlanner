using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Identity.Api
{
    public static class IdentityConfigs
    {
        private const string CLIENTID = "mealplanner_client";
        private const string APISCOPE = "mealplanner_api"; 

        public static IEnumerable<Client> GetClients() =>
            new List<Client>
            {
                new Client
                {
                    ClientId = CLIENTID,
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets = { new Secret(Common.Constants.MealPlanner.SigningKey.Sha256()) },
                    AllowedScopes = { APISCOPE, IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile }
                }
            };

        public static IEnumerable<ApiScope> GetApiScopes() =>
            new List<ApiScope>
            {
                new ApiScope(APISCOPE, "MealPlanner API")
            };

        public static IEnumerable<ApiResource> GetApiResources() =>
            new List<ApiResource>
            {
                new ApiResource(APISCOPE, "MealPlanner API")
                {
                    Scopes = { APISCOPE }
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
