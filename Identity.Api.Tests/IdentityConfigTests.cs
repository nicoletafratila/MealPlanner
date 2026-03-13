using System.Security.Claims;
using Duende.IdentityServer.Models;

namespace Identity.Api.Tests
{
    public class IdentityConfigsTests
    {
        [Test]
        public void GetClients_ShouldReturnValidClient()
        {
            var clients = IdentityConfigs.GetClients().ToList();

            Assert.That(clients, Has.Count.EqualTo(1));

            var client = clients.First();
            Assert.Multiple(() =>
            {
                Assert.That(client.ClientId, Is.EqualTo("mealplanner_client"));
                Assert.That(client.AllowedGrantTypes, Is.EqualTo(GrantTypes.ResourceOwnerPassword));
                Assert.That(client.AllowedScopes.Contains(Common.Constants.MealPlanner.ApiScope), Is.True);
                Assert.That(client.AllowedScopes.Contains("roles"), Is.True);
                Assert.That(client.ClientSecrets, Is.Not.Empty);
            });
        }

        [Test]
        public void GetApiScopes_ShouldReturnValidScope()
        {
            var scopes = IdentityConfigs.GetApiScopes().ToList();

            Assert.That(scopes, Has.Count.EqualTo(1));

            var scope = scopes.First();
            Assert.Multiple(() =>
            {
                Assert.That(scope.Name, Is.EqualTo(Common.Constants.MealPlanner.ApiScope));
                Assert.That(scope.DisplayName, Is.EqualTo("MealPlanner API"));

                Assert.That(scope.UserClaims.Contains(ClaimTypes.Role), Is.True);
                Assert.That(scope.UserClaims.Contains(ClaimTypes.Name), Is.True);
            });
        }

        [Test]
        public void GetApiResources_ShouldReturnValidApiResource()
        {
            var resources = IdentityConfigs.GetApiResources().ToList();

            Assert.That(resources, Has.Count.EqualTo(1));

            var apiResource = resources.First();
            Assert.Multiple(() =>
            {
                Assert.That(apiResource.Name, Is.EqualTo(Common.Constants.MealPlanner.ApiScope));
                Assert.That(apiResource.Scopes.Contains(Common.Constants.MealPlanner.ApiScope), Is.True);

                Assert.That(apiResource.UserClaims.Contains(ClaimTypes.Role), Is.True);
                Assert.That(apiResource.UserClaims.Contains(ClaimTypes.Name), Is.True);
            });
        }

        [Test]
        public void GetIdentityResources_ShouldContainOpenIdProfileAndRoles()
        {
            var idResources = IdentityConfigs.GetIdentityResources().ToList();

            Assert.That(idResources, Has.Count.EqualTo(3));

            Assert.Multiple(() =>
            {
                Assert.That(idResources.Any(r => r.Name == "openid"), Is.True);
                Assert.That(idResources.Any(r => r.Name == "profile"), Is.True);
            });

            var roles = idResources.FirstOrDefault(r => r.Name == "roles");

            Assert.That(roles, Is.Not.Null);
            Assert.That(roles!.UserClaims.Contains(ClaimTypes.Role), Is.True);
        }
    }
}