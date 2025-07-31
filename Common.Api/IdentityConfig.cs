using Common.Constants;
using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Common.Api
{
    public static class IdentityConfig
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
             //new IdentityResource[]
             //{
             //    //new IdentityResources.OpenId(),
             //    //new IdentityResources.Profile(),
             //    //new IdentityResources.Email(),
             //    //new IdentityResource
             //    //{
             //    //    Name = "role",
             //    //    UserClaims = new List<string> {
             //    //        JwtClaimTypes.Id,
             //    //        JwtClaimTypes.Name,
             //    //        JwtClaimTypes.GivenName,
             //    //        JwtClaimTypes.FamilyName,
             //    //        JwtClaimTypes.Email,
             //    //        JwtClaimTypes.Role,
             //    //        JwtClaimTypes.WebSite
             //    //    }
             //    //}

             //    new IdentityResources.OpenId(),
             //    new IdentityResources.Profile()
             //};
             new IdentityResource[]
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile(),
                    new IdentityResources.Email(),
                    new IdentityResource
                    {
                        Name = "role",
                        UserClaims = new List<string> {"role"}
                    }
                };

        public static IEnumerable<ApiScope> ApiScopes =>
                    new ApiScope[]
                    {
                        new ApiScope($"{ApiConfigNames.RecipeBook}.read", $"Read Access to {ApiConfigNames.RecipeBook}"),
                        new ApiScope($"{ApiConfigNames.RecipeBook}.write", $"Write Access to {ApiConfigNames.RecipeBook}")
                  //new ApiScope(ApiConfigNames.RecipeBook),
                  // new ApiScope(ApiConfigNames.MealPlanner),
                  // new ApiScope(IdentityServerConstants.LocalApi.ScopeName)
                  //      //new ApiScope("MealPlanner.Api.full"), 
                  //      //               new ApiScope(IdentityServerConstants.LocalApi.ScopeName)
                    };

        public static IEnumerable<ApiResource> ApiResources => new[]
        {
            //new ApiResource("MealPlanner.Api","MealPlanner Api")
            //{
            //    ApiSecrets = { new Secret("secret".Sha256()) },
            //    Scopes = new List<string> { "MealPlanner.Api.full" },
            //    UserClaims = {
            //        JwtClaimTypes.Id,
            //        JwtClaimTypes.Name,
            //        JwtClaimTypes.GivenName,
            //        JwtClaimTypes.FamilyName,
            //        //JwtClaimTypes.Subject,
            //        //JwtClaimTypes.Profile,
            //        JwtClaimTypes.Email,
            //        JwtClaimTypes.Role,
            //        JwtClaimTypes.WebSite
            //    }
            //}
            new ApiResource
            {
                Name = ApiConfigNames.RecipeBook,
                DisplayName = ApiConfigNames.RecipeBook,
                Description = ApiConfigNames.RecipeBook,
                Scopes = new List<string> {$"{ApiConfigNames.RecipeBook}.read", $"{ApiConfigNames.RecipeBook}.write"},
                ApiSecrets = new List<Secret> {new Secret(MealPlannerKey.SigningKey.Sha256())},
                UserClaims = new List<string> {"role"}
            }
        };

        public static IEnumerable<Client> Clients =>
            new[]
            {
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

                //new Client
                //{
                //    //ClientId = "blazor_wasm",
                //    //AllowedGrantTypes = GrantTypes.Code,
                //    //RequirePkce = true,
                //    //RequireClientSecret = false,
                //    //RedirectUris = { "https://localhost:5001/authentication/login-callback" },
                //    //PostLogoutRedirectUris = { "https://localhost:5001/authentication/logout-callback" },
                //    //AllowedScopes = { "openid", "profile", ApiConfigNames.RecipeBook, ApiConfigNames.MealPlanner, IdentityServerConstants.LocalApi.ScopeName },
                //    //AllowAccessTokensViaBrowser = true

                //    ClientId = "blazor_client",
                //    AllowedGrantTypes = GrantTypes.Code,
                //    RequirePkce = true,
                //    RequireClientSecret = false,
                //    RedirectUris =           { "https://localhost:5002/authentication/login-callback" },
                //    PostLogoutRedirectUris = { "https://localhost:5002/" },
                //    AllowedScopes = { "openid", "profile", ApiConfigNames.RecipeBook, ApiConfigNames.MealPlanner, IdentityServerConstants.LocalApi.ScopeName },
                //    AllowAccessTokensViaBrowser = true
                //}
                //new Client
                //{
                //    //ClientId = "oauthClient",
                //    //ClientName = "Example client application using client credentials",
                //    //AllowedGrantTypes = GrantTypes.ClientCredentials,
                //    //ClientSecrets = new List<Secret> {new Secret(MealPlannerKey.SigningKey.Sha256())},
                //    //AllowedScopes = new List<string> { $"{ApiConfigNames.RecipeBook}.read" }
                //    ClientId = "oidcClient",
                //    ClientName = "Example Client Application",
                //    ClientSecrets = new List<Secret> {new Secret(MealPlannerKey.SigningKey.Sha256())},
    
                //    AllowedGrantTypes = GrantTypes.Code,
                //    RedirectUris = new List<string> {"https://localhost:5002/signin-oidc"},
                //    AllowedScopes = new List<string>
                //    {
                //        IdentityServerConstants.StandardScopes.OpenId,
                //        IdentityServerConstants.StandardScopes.Profile,
                //        IdentityServerConstants.StandardScopes.Email,
                //        "role",
                //        "api1.read"
                //    },

                //    RequirePkce = true,
                //    AllowPlainTextPkce = false
                //}

                new Client
                {
                    ClientId = "blazor-wasm",
                    ClientName = "Blazor WASM Client",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    RedirectUris =
                    {
                        "https://localhost:5002/authentication/login-callback",
                        "https://localhost:5002/authentication/logout-callback"
                    },
                    PostLogoutRedirectUris = { "https://localhost:5002/" },
                    AllowedScopes = { "openid", "profile", ApiConfigNames.RecipeBook },
                    AllowAccessTokensViaBrowser = true
                }
            };
    }
}
