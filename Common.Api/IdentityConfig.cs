using Common.Constants;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Common.Api
{
    public static class IdentityConfig
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                //new IdentityResources.Email(),
                //new IdentityResource
                //{
                //    Name = "role",
                //    UserClaims = new List<string> { 
                //JwtRegisteredClaimNames.Sub
                //        JwtClaimTypes.Id,
                //        JwtClaimTypes.Name,
                //        JwtClaimTypes.GivenName,
                //        JwtClaimTypes.FamilyName,
                //        JwtClaimTypes.Email,
                //        JwtClaimTypes.Role,
                //        JwtClaimTypes.WebSite 
                //    }
                //}
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("MealPlanner.Api"),
                new ApiScope("RecipeBook.Api"),
                new ApiScope(IdentityServerConstants.LocalApi.ScopeName)
            };

        public static IEnumerable<ApiResource> ApiResources => new[]
        {
            new ApiResource("MealPlanner.Api", "MealPlanner Api")
            {
                ApiSecrets = { new Secret(MealPlannerKey.SigningKey.Sha256()) },
                Scopes = new List<string> { "MealPlanner.Api" },
                //UserClaims = {
                //    JwtClaimTypes.Id,
                //    JwtClaimTypes.Name,
                //    JwtClaimTypes.GivenName,
                //    JwtClaimTypes.FamilyName,
                //    //JwtClaimTypes.Subject,
                //    //JwtClaimTypes.Profile,
                //    JwtClaimTypes.Email,
                //    JwtClaimTypes.Role,
                //    JwtClaimTypes.WebSite
                //}
            },
            new ApiResource("RecipeBook.Api", "RecipeBook Api")
            {
                ApiSecrets = { new Secret(MealPlannerKey.SigningKey.Sha256()) },
                Scopes = new List<string> { "RecipeBook.Api" },
                //UserClaims = {
                //    JwtClaimTypes.Id,
                //    JwtClaimTypes.Name,
                //    JwtClaimTypes.GivenName,
                //    JwtClaimTypes.FamilyName,
                //    //JwtClaimTypes.Subject,
                //    //JwtClaimTypes.Profile,
                //    JwtClaimTypes.Email,
                //    JwtClaimTypes.Role,
                //    JwtClaimTypes.WebSite
                //}
            },
            new ApiResource(IdentityServerConstants.LocalApi.ScopeName, "IdentityServer Api")
            {
                ApiSecrets = { new Secret(MealPlannerKey.SigningKey.Sha256()) },
                Scopes = new List<string> { IdentityServerConstants.LocalApi.ScopeName },
                //UserClaims = {
                //    JwtClaimTypes.Id,
                //    JwtClaimTypes.Name,
                //    JwtClaimTypes.GivenName,
                //    JwtClaimTypes.FamilyName,
                //    //JwtClaimTypes.Subject,
                //    //JwtClaimTypes.Profile,
                //    JwtClaimTypes.Email,
                //    JwtClaimTypes.Role,
                //    JwtClaimTypes.WebSite
                //}
            }
        };

        public static IEnumerable<Client> Clients =>
            new[]
            {
                new Client
                {
                    ClientId = "MealPlanner",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword, 
                    ClientSecrets = { new Secret(MealPlannerKey.SigningKey.Sha256()) },
                    AllowedScopes = { "openid", "profile", "MealPlanner.Api", "RecipeBook.Api", IdentityServerConstants.LocalApi.ScopeName },
                    AllowOfflineAccess = true 
                }
                //new Client
                //{
                //    ClientId = "MealPlanner",
                //    ClientName = "Meal Planner",

                //    RedirectUris = { "https://localhost:7118/signin-oidc", "https://localhost:7118/", "https://localhost:7118/authentication/login-callback", "https://localhost:7065/signin-oidc", "https://localhost:7118/authentication/signout-callback-oidc" },
                //    PostLogoutRedirectUris = { "https://localhost:5001/signout-callback-oidc"},

                //    RequireClientSecret = false,

                //    AllowedGrantTypes = GrantTypes.Code,
                //    AllowedScopes = { "openid", "profile", "email", "MealPlanner.Api.full", "role", IdentityServerConstants.LocalApi.ScopeName},

                //    AllowOfflineAccess = true,
                //    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                //    RefreshTokenExpiration = TokenExpiration.Sliding
                //}
            };
    }
}
