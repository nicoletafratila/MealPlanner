using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Common.Http;
using MealPlanner.UI.Mobile.Services;
using Moq;

namespace MealPlanner.UI.Mobile.Tests.Services
{
    [TestFixture]
    public class MobileAuthStateServiceTests
    {
        private Mock<ITokenProvider> _tokenProviderMock = null!;
        private MobileAuthStateService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _tokenProviderMock = new Mock<ITokenProvider>(MockBehavior.Strict);
            _service = new MobileAuthStateService(_tokenProviderMock.Object);
        }

        private void SetupToken(string? token) =>
            _tokenProviderMock.Setup(t => t.GetTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(token);

        private static string Base64UrlEncode(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static string CreateJwt(Dictionary<string, object> payload)
        {
            var header = Base64UrlEncode("{\"alg\":\"HS256\",\"typ\":\"JWT\"}");
            var payloadEncoded = Base64UrlEncode(JsonSerializer.Serialize(payload));
            var signature = Base64UrlEncode("signature");
            return $"{header}.{payloadEncoded}.{signature}";
        }

        [Test]
        public async Task GetCurrentUserAsync_NullToken_ReturnsUnauthenticatedPrincipal()
        {
            SetupToken(null);

            var user = await _service.GetCurrentUserAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(user.Identity?.IsAuthenticated, Is.False);
                Assert.That(user.Identity?.AuthenticationType, Is.Null.Or.Empty);
            }
        }

        [Test]
        public async Task GetCurrentUserAsync_EmptyToken_ReturnsUnauthenticatedPrincipal()
        {
            SetupToken(string.Empty);

            var user = await _service.GetCurrentUserAsync();

            Assert.That(user.Identity?.IsAuthenticated, Is.False);
        }

        [Test]
        public async Task GetCurrentUserAsync_ValidNonExpiredToken_ReturnsAuthenticatedPrincipalWithClaims()
        {
            var token = CreateJwt(new Dictionary<string, object>
            {
                ["name"] = "Bob",
                ["role"] = "admin",
                ["exp"] = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()
            });
            SetupToken(token);

            var user = await _service.GetCurrentUserAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(user.Identity?.IsAuthenticated, Is.True);
                Assert.That(user.Identity?.AuthenticationType, Is.EqualTo("jwt"));
                Assert.That(user.FindFirst("name")?.Value, Is.EqualTo("Bob"));
                Assert.That(user.FindFirst("role")?.Value, Is.EqualTo("admin"));
            }
        }

        [Test]
        public async Task GetCurrentUserAsync_ExpiredExpClaim_ReturnsUnauthenticatedPrincipal()
        {
            var token = CreateJwt(new Dictionary<string, object>
            {
                ["name"] = "Alice",
                ["exp"] = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds()
            });
            SetupToken(token);

            var user = await _service.GetCurrentUserAsync();

            Assert.That(user.Identity?.IsAuthenticated, Is.False);
        }

        [Test]
        public async Task GetCurrentUserAsync_NonExpiredExpClaim_ReturnsAuthenticatedPrincipal()
        {
            var token = CreateJwt(new Dictionary<string, object>
            {
                ["name"] = "Alice",
                ["exp"] = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()
            });
            SetupToken(token);

            var user = await _service.GetCurrentUserAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(user.Identity?.IsAuthenticated, Is.True);
                Assert.That(user.FindFirst("name")?.Value, Is.EqualTo("Alice"));
            }
        }

        [Test]
        public async Task GetCurrentUserAsync_MalformedTokenWithInvalidBase64Payload_ReturnsUnauthenticatedWithoutThrowing()
        {
            SetupToken("aaa.!!!not-base64!!!.bbb");

            ClaimsPrincipal? user = null;
            Assert.DoesNotThrowAsync(async () => user = await _service.GetCurrentUserAsync());

            Assert.That(user?.Identity?.IsAuthenticated, Is.False);
        }

        [Test]
        public async Task GetCurrentUserAsync_TokenWithWrongSegmentCount_ReturnsUnauthenticatedPrincipal()
        {
            SetupToken("only.two-segments");

            var user = await _service.GetCurrentUserAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(user.Identity?.IsAuthenticated, Is.False);
                Assert.That(user.Claims, Is.Empty);
            }
        }

        [Test]
        public async Task IsAuthenticatedAsync_ValidToken_ReturnsTrue()
        {
            var token = CreateJwt(new Dictionary<string, object> { ["name"] = "Bob" });
            SetupToken(token);

            var result = await _service.IsAuthenticatedAsync();

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsAuthenticatedAsync_NoToken_ReturnsFalse()
        {
            SetupToken(null);

            var result = await _service.IsAuthenticatedAsync();

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task IsAuthenticatedAsync_MalformedTokenWithInvalidBase64Payload_ReturnsFalse()
        {
            SetupToken("aaa.!!!not-base64!!!.bbb");

            var result = await _service.IsAuthenticatedAsync();

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task GetUserNameAsync_TokenWithNameClaim_ReturnsNameClaimValue()
        {
            // Identity.Name resolves against the default ClaimTypes.Name URI, which our raw "name"
            // claim does not match, so the fallback to FindFirst("name") is what actually supplies
            // the value here.
            var token = CreateJwt(new Dictionary<string, object> { ["name"] = "Charlie" });
            SetupToken(token);

            var result = await _service.GetUserNameAsync();

            Assert.That(result, Is.EqualTo("Charlie"));
        }

        [Test]
        public async Task GetUserNameAsync_TokenWithOnlySubClaim_ReturnsSubClaimValue()
        {
            var token = CreateJwt(new Dictionary<string, object> { ["sub"] = "user-123" });
            SetupToken(token);

            var result = await _service.GetUserNameAsync();

            Assert.That(result, Is.EqualTo("user-123"));
        }

        [Test]
        public async Task GetUserNameAsync_NoToken_ReturnsNull()
        {
            SetupToken(null);

            var result = await _service.GetUserNameAsync();

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetUserNameAsync_TokenWithNoNameOrSubClaim_ReturnsNull()
        {
            var token = CreateJwt(new Dictionary<string, object> { ["role"] = "admin" });
            SetupToken(token);

            var result = await _service.GetUserNameAsync();

            Assert.That(result, Is.Null);
        }

        [Test]
        public void NotifyAuthStateChanged_RaisesAuthStateChangedEvent()
        {
            var wasRaised = false;
            _service.AuthStateChanged += () => wasRaised = true;

            _service.NotifyAuthStateChanged();

            Assert.That(wasRaised, Is.True);
        }

        [Test]
        public void NotifyAuthStateChanged_NoSubscribers_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _service.NotifyAuthStateChanged());
        }
    }
}
