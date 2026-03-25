using System.Net.Http.Headers;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Moq;

namespace Common.Api.Tests
{
    [TestFixture]
    public class HttpClientExtensionsTests
    {
        [Test]
        public void EnsureAuthorizationHeader_NullHttpClient_ThrowsArgumentNullException()
        {
            HttpClient? client = null;

            Assert.That(
                () => HttpClientExtensions.EnsureAuthorizationHeader(client!, "token"),
                Throws.TypeOf<ArgumentNullException>()
                      .With.Property(nameof(ArgumentNullException.ParamName))
                      .EqualTo("httpClient"));
        }

        [Test]
        public void EnsureAuthorizationHeader_DoesNothing_WhenTokenIsNullOrWhitespace()
        {
            using var client = new HttpClient();

            HttpClientExtensions.EnsureAuthorizationHeader(client, null);
            Assert.That(client.DefaultRequestHeaders.Authorization, Is.Null);

            HttpClientExtensions.EnsureAuthorizationHeader(client, string.Empty);
            Assert.That(client.DefaultRequestHeaders.Authorization, Is.Null);

            HttpClientExtensions.EnsureAuthorizationHeader(client, "   ");
            Assert.That(client.DefaultRequestHeaders.Authorization, Is.Null);
        }

        [Test]
        public void EnsureAuthorizationHeader_SetsHeader_WhenTokenProvided_AndNoExistingHeader()
        {
            using var client = new HttpClient();
            const string token = "abc123";

            client.EnsureAuthorizationHeader(token);

            var auth = client.DefaultRequestHeaders.Authorization;
            Assert.That(auth, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(auth!.Scheme, Is.EqualTo(JwtBearerDefaults.AuthenticationScheme));
                Assert.That(auth.Parameter, Is.EqualTo(token));
            });
        }

        [Test]
        public void EnsureAuthorizationHeader_DoesNotChangeHeader_WhenSameTokenAlreadySet()
        {
            using var client = new HttpClient();
            const string token = "same-token";

            var initialHeader = new AuthenticationHeaderValue(
                JwtBearerDefaults.AuthenticationScheme,
                token);

            client.DefaultRequestHeaders.Authorization = initialHeader;

            client.EnsureAuthorizationHeader(token);

            Assert.That(ReferenceEquals(initialHeader, client.DefaultRequestHeaders.Authorization), Is.True);
        }

        [Test]
        public void EnsureAuthorizationHeader_UpdatesHeader_WhenDifferentTokenProvided()
        {
            using var client = new HttpClient();
            const string oldToken = "old-token";
            const string newToken = "new-token";

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, oldToken);

            client.EnsureAuthorizationHeader(newToken);

            var auth = client.DefaultRequestHeaders.Authorization;
            Assert.That(auth, Is.Not.Null);
            Assert.That(auth!.Parameter, Is.EqualTo(newToken));
        }

        [Test]
        public void GetCleanToken_NullOrEmpty_ReturnsEmptyString()
        {
            Assert.Multiple(() =>
            {
                Assert.That(HttpClientExtensions.GetCleanToken(null), Is.EqualTo(string.Empty));
                Assert.That(HttpClientExtensions.GetCleanToken(string.Empty), Is.EqualTo(string.Empty));
                Assert.That(HttpClientExtensions.GetCleanToken("   "), Is.EqualTo(string.Empty));
            });
        }

        [Test]
        public void GetCleanToken_WithoutBearerScheme_ReturnsEmptyString()
        {
            Assert.That(HttpClientExtensions.GetCleanToken("just-a-token"), Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetCleanToken_WithBearerScheme_ReturnsToken_PreservesContent()
        {
            const string token = "abc.def.ghi";

            var result = HttpClientExtensions.GetCleanToken($"Bearer {token}");

            Assert.That(result, Is.EqualTo(token));
        }

        [Test]
        public void GetCleanToken_WithBearerScheme_CaseInsensitive()
        {
            const string token = "xyz";

            Assert.Multiple(() =>
            {
                Assert.That(HttpClientExtensions.GetCleanToken($"bearer {token}"), Is.EqualTo(token));
                Assert.That(HttpClientExtensions.GetCleanToken($"BEARER {token}"), Is.EqualTo(token));
            });
        }

        [Test]
        public void EnsureAuthorizationHeaderAsync_NullHttpClient_ThrowsArgumentNullException()
        {
            HttpClient? client = null;
            var sessionStorage = new Mock<ISessionStorageService>().Object;
            var tokenProvider = new TokenProvider(sessionStorage);

            Assert.That(
                async () => await HttpClientExtensions.EnsureAuthorizationHeaderAsync(client!, tokenProvider, CancellationToken.None),
                Throws.TypeOf<ArgumentNullException>()
                      .With.Property(nameof(ArgumentNullException.ParamName))
                      .EqualTo("httpClient"));
        }

        [Test]
        public void EnsureAuthorizationHeaderAsync_NullTokenProvider_ThrowsArgumentNullException()
        {
            using var client = new HttpClient();
            TokenProvider? tokenProvider = null;

            Assert.That(
                async () => await client.EnsureAuthorizationHeaderAsync(tokenProvider!, CancellationToken.None),
                Throws.TypeOf<ArgumentNullException>()
                      .With.Property(nameof(ArgumentNullException.ParamName))
                      .EqualTo("tokenProvider"));
        }

        [Test]
        public void EnsureAuthorizationHeaderAsync_CancelledToken_ThrowsOperationCanceledException()
        {
            using var client = new HttpClient();

            var sessionStorageMock = new Mock<ISessionStorageService>(MockBehavior.Loose);
            var tokenProvider = new TokenProvider(sessionStorageMock.Object);

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.That(
                async () => await client.EnsureAuthorizationHeaderAsync(tokenProvider, cts.Token),
                Throws.InstanceOf<OperationCanceledException>());
        }

        [Test]
        public async Task EnsureAuthorizationHeaderAsync_SetsAuthorizationHeader_FromTokenProvider()
        {
            using var client = new HttpClient();
            const string token = "async-token";

            var sessionStorageMock = new Mock<ISessionStorageService>(MockBehavior.Strict);
            sessionStorageMock
                .Setup(s => s.GetItemAsync<string?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            var tokenProvider = new TokenProvider(sessionStorageMock.Object);

            await client.EnsureAuthorizationHeaderAsync(tokenProvider, CancellationToken.None);

            var auth = client.DefaultRequestHeaders.Authorization;
            Assert.That(auth, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(auth!.Scheme, Is.EqualTo(JwtBearerDefaults.AuthenticationScheme));
                Assert.That(auth.Parameter, Is.EqualTo(token));
            });
        }
    }
}