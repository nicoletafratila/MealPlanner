using System.Net.Http.Headers;
using Moq;

namespace Common.Http.Tests
{
    [TestFixture]
    public class HttpClientExtensionsTests
    {
        // ── EnsureAuthorizationHeader (sync) ────────────────────────────────

        [Test]
        public void EnsureAuthorizationHeader_SetsBearer_WhenNoHeaderPresent()
        {
            var client = new HttpClient();
            client.EnsureAuthorizationHeader("my-token");
            Assert.That(client.DefaultRequestHeaders.Authorization?.Scheme, Is.EqualTo("Bearer"));
            Assert.That(client.DefaultRequestHeaders.Authorization?.Parameter, Is.EqualTo("my-token"));
        }

        [Test]
        public void EnsureAuthorizationHeader_UpdatesHeader_WhenTokenDiffers()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "old-token");

            client.EnsureAuthorizationHeader("new-token");

            Assert.That(client.DefaultRequestHeaders.Authorization?.Parameter, Is.EqualTo("new-token"));
        }

        [Test]
        public void EnsureAuthorizationHeader_DoesNotUpdate_WhenTokenMatches()
        {
            var client = new HttpClient();
            var original = new AuthenticationHeaderValue("Bearer", "same-token");
            client.DefaultRequestHeaders.Authorization = original;

            client.EnsureAuthorizationHeader("same-token");

            Assert.That(client.DefaultRequestHeaders.Authorization, Is.SameAs(original));
        }

        [Test]
        public void EnsureAuthorizationHeader_DoesNotSetHeader_WhenTokenIsNull()
        {
            var client = new HttpClient();
            client.EnsureAuthorizationHeader(null);
            Assert.That(client.DefaultRequestHeaders.Authorization, Is.Null);
        }

        [Test]
        public void EnsureAuthorizationHeader_DoesNotSetHeader_WhenTokenIsEmpty()
        {
            var client = new HttpClient();
            client.EnsureAuthorizationHeader(string.Empty);
            Assert.That(client.DefaultRequestHeaders.Authorization, Is.Null);
        }

        [Test]
        public void EnsureAuthorizationHeader_DoesNotSetHeader_WhenTokenIsWhitespace()
        {
            var client = new HttpClient();
            client.EnsureAuthorizationHeader("   ");
            Assert.That(client.DefaultRequestHeaders.Authorization, Is.Null);
        }

        [Test]
        public void EnsureAuthorizationHeader_Throws_WhenHttpClientIsNull()
        {
            HttpClient? client = null;
            Assert.Throws<ArgumentNullException>(() => client!.EnsureAuthorizationHeader("token"));
        }

        // ── EnsureAuthorizationHeaderAsync ──────────────────────────────────

        [Test]
        public async Task EnsureAuthorizationHeaderAsync_FetchesToken_AndSetsHeader()
        {
            var provider = new Mock<ITokenProvider>();
            provider.Setup(p => p.GetTokenAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync("jwt-token");

            var client = new HttpClient();
            await client.EnsureAuthorizationHeaderAsync(provider.Object, CancellationToken.None);

            Assert.That(client.DefaultRequestHeaders.Authorization?.Scheme, Is.EqualTo("Bearer"));
            Assert.That(client.DefaultRequestHeaders.Authorization?.Parameter, Is.EqualTo("jwt-token"));
        }

        [Test]
        public async Task EnsureAuthorizationHeaderAsync_DoesNotSetHeader_WhenProviderReturnsNull()
        {
            var provider = new Mock<ITokenProvider>();
            provider.Setup(p => p.GetTokenAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync((string?)null);

            var client = new HttpClient();
            await client.EnsureAuthorizationHeaderAsync(provider.Object, CancellationToken.None);

            Assert.That(client.DefaultRequestHeaders.Authorization, Is.Null);
        }

        [Test]
        public void EnsureAuthorizationHeaderAsync_Throws_WhenHttpClientIsNull()
        {
            HttpClient? client = null;
            var provider = new Mock<ITokenProvider>();
            Assert.ThrowsAsync<ArgumentNullException>(
                async () => await client!.EnsureAuthorizationHeaderAsync(provider.Object, CancellationToken.None));
        }

        [Test]
        public void EnsureAuthorizationHeaderAsync_Throws_WhenTokenProviderIsNull()
        {
            var client = new HttpClient();
            Assert.ThrowsAsync<ArgumentNullException>(
                async () => await client.EnsureAuthorizationHeaderAsync(null!, CancellationToken.None));
        }

        [Test]
        public void EnsureAuthorizationHeaderAsync_Throws_WhenCancellationAlreadyRequested()
        {
            var provider = new Mock<ITokenProvider>();
            var client = new HttpClient();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.ThrowsAsync<OperationCanceledException>(
                async () => await client.EnsureAuthorizationHeaderAsync(provider.Object, cts.Token));
        }

        // ── GetCleanToken ────────────────────────────────────────────────────

        [Test]
        public void GetCleanToken_ExtractsToken_FromBearerHeader()
        {
            var result = HttpClientExtensions.GetCleanToken("Bearer my-secret-token");
            Assert.That(result, Is.EqualTo("my-secret-token"));
        }

        [Test]
        public void GetCleanToken_IsCaseInsensitive()
        {
            Assert.That(HttpClientExtensions.GetCleanToken("BEARER my-token"), Is.EqualTo("my-token"));
            Assert.That(HttpClientExtensions.GetCleanToken("bearer my-token"), Is.EqualTo("my-token"));
        }

        [Test]
        public void GetCleanToken_TrimsLeadingAndTrailingWhitespace()
        {
            var result = HttpClientExtensions.GetCleanToken("Bearer   padded-token   ");
            Assert.That(result, Is.EqualTo("padded-token"));
        }

        [Test]
        public void GetCleanToken_ReturnsEmpty_WhenNull()
        {
            Assert.That(HttpClientExtensions.GetCleanToken(null), Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetCleanToken_ReturnsEmpty_WhenEmpty()
        {
            Assert.That(HttpClientExtensions.GetCleanToken(string.Empty), Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetCleanToken_ReturnsEmpty_WhenWhitespace()
        {
            Assert.That(HttpClientExtensions.GetCleanToken("   "), Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetCleanToken_ReturnsEmpty_WhenNoBearerPrefix()
        {
            Assert.That(HttpClientExtensions.GetCleanToken("Basic dXNlcjpwYXNz"), Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetCleanToken_ReturnsEmpty_WhenJustBearerKeyword()
        {
            Assert.That(HttpClientExtensions.GetCleanToken("Bearer"), Is.EqualTo(string.Empty));
        }
    }
}
