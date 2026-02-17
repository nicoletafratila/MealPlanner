using System.Text;
using System.Text.Json;
using Blazored.SessionStorage;
using MealPlanner.UI.Web.Services.Identities;
using Moq;

namespace MealPlanner.UI.Web.Tests.Services.Identities
{
    [TestFixture]
    public class CustomAuthenticationStateTests
    {
        private static string CreateJwtToken(
            IDictionary<string, object> extraClaims,
            DateTimeOffset? expiresAtUtc = null)
        {
            var header = new Dictionary<string, object>
            {
                ["alg"] = "none",
                ["typ"] = "JWT"
            };

            var exp = expiresAtUtc ?? DateTimeOffset.UtcNow.AddHours(1);
            var payload = new Dictionary<string, object>(extraClaims)
            {
                ["exp"] = exp.ToUnixTimeSeconds()
            };

            static string Base64UrlEncode(string json)
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                return Convert.ToBase64String(bytes)
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');
            }

            var headerJson = JsonSerializer.Serialize(header);
            var payloadJson = JsonSerializer.Serialize(payload);

            var headerPart = Base64UrlEncode(headerJson);
            var payloadPart = Base64UrlEncode(payloadJson);

            return $"{headerPart}.{payloadPart}.signature";
        }

        [Test]
        public async Task GetAuthenticationStateAsync_ReturnsAnonymous_WhenTokenMissing()
        {
            // Arrange
            var sessionStorage = new Mock<ISessionStorageService>();

            sessionStorage
                .Setup(s => s.GetItemAsync<string?>(Common.Constants.MealPlanner.AuthToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            var provider = new CustomAuthenticationState(sessionStorage.Object);

            // Act
            var state = await provider.GetAuthenticationStateAsync();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(state.User.Identity, Is.Not.Null);
                Assert.That(state.User.Identity!.IsAuthenticated, Is.False);
                Assert.That(state.User.Claims, Is.Empty);
            });
        }

        [Test]
        public async Task GetAuthenticationStateAsync_ReturnsAnonymous_WhenTokenExpired()
        {
            // Arrange
            var sessionStorage = new Mock<ISessionStorageService>();

            var token = CreateJwtToken(
                new Dictionary<string, object> { ["name"] = "expired-user" },
                expiresAtUtc: DateTimeOffset.UtcNow.AddSeconds(-10)); // already expired

            sessionStorage
                .Setup(s => s.GetItemAsync<string?>(Common.Constants.MealPlanner.AuthToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            var provider = new CustomAuthenticationState(sessionStorage.Object);

            // Act
            var state = await provider.GetAuthenticationStateAsync();

            // Assert
            Assert.That(state.User.Identity, Is.Not.Null);
            Assert.That(state.User.Identity!.IsAuthenticated, Is.False);
        }

        [Test]
        public async Task GetAuthenticationStateAsync_ReturnsAuthenticatedUser_WithSimpleClaims()
        {
            // Arrange
            var sessionStorage = new Mock<ISessionStorageService>();

            var token = CreateJwtToken(new Dictionary<string, object>
            {
                ["sub"] = "user-123",
                ["name"] = "Alice",
                ["role"] = "Admin"
            });

            sessionStorage
                .Setup(s => s.GetItemAsync<string?>(Common.Constants.MealPlanner.AuthToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            var provider = new CustomAuthenticationState(sessionStorage.Object);

            // Act
            var state = await provider.GetAuthenticationStateAsync();
            var user = state.User;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(user.Identity, Is.Not.Null);
                Assert.That(user.Identity!.IsAuthenticated, Is.True);
                Assert.That(user.Identity.AuthenticationType, Is.EqualTo("jwt"));

                Assert.That(user.FindFirst("sub")?.Value, Is.EqualTo("user-123"));
                Assert.That(user.FindFirst("name")?.Value, Is.EqualTo("Alice"));
                Assert.That(user.FindFirst("role")?.Value, Is.EqualTo("Admin"));
            });
        }

        [Test]
        public async Task GetAuthenticationStateAsync_CreatesMultipleClaims_ForArrayClaim()
        {
            // Arrange
            var sessionStorage = new Mock<ISessionStorageService>();

            var token = CreateJwtToken(new Dictionary<string, object>
            {
                ["name"] = "Bob",
                ["roles"] = new[] { "User", "Manager" }
            });

            sessionStorage
                .Setup(s => s.GetItemAsync<string?>(Common.Constants.MealPlanner.AuthToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            var provider = new CustomAuthenticationState(sessionStorage.Object);

            // Act
            var state = await provider.GetAuthenticationStateAsync();
            var user = state.User;

            // Assert
            var roleClaims = user.FindAll("roles").Select(c => c.Value).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(user.Identity, Is.Not.Null);
                Assert.That(user.Identity!.IsAuthenticated, Is.True);
                Assert.That(user.FindFirst("name")?.Value, Is.EqualTo("Bob"));
                Assert.That(roleClaims, Is.EquivalentTo(new[] { "User", "Manager" }));
            });
        }

        [Test]
        public async Task GetAuthenticationStateAsync_ReturnsAnonymous_WhenTokenMalformed()
        {
            // Arrange
            var sessionStorage = new Mock<ISessionStorageService>();

            // Not a valid JWT (no dots)
            const string malformedToken = "not-a-jwt-token";

            sessionStorage
                .Setup(s => s.GetItemAsync<string?>(Common.Constants.MealPlanner.AuthToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(malformedToken);

            var provider = new CustomAuthenticationState(sessionStorage.Object);

            // Act
            var state = await provider.GetAuthenticationStateAsync();

            // Assert
            Assert.That(state.User.Identity, Is.Not.Null);
            Assert.That(state.User.Identity!.IsAuthenticated, Is.False);
        }
    }
}