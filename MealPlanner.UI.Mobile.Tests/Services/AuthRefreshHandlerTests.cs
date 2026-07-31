using System.Net;
using System.Net.Http.Headers;
using Common.Http;
using Identity.Services.Http;
using MealPlanner.UI.Mobile.Services;
using Moq;

namespace MealPlanner.UI.Mobile.Tests.Services
{
    [TestFixture]
    public class AuthRefreshHandlerTests
    {
        private sealed class StubHandler : DelegatingHandler
        {
            private readonly Queue<HttpResponseMessage> _responses;

            public StubHandler(params HttpResponseMessage[] responses)
            {
                _responses = new Queue<HttpResponseMessage>(responses);
            }

            public List<HttpRequestMessage> CapturedRequests { get; } = [];

            public int CallCount { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                CallCount++;
                CapturedRequests.Add(request);
                var response = _responses.Count > 0
                    ? _responses.Dequeue()
                    : new HttpResponseMessage(HttpStatusCode.InternalServerError);
                return Task.FromResult(response);
            }
        }

        private Mock<IServiceProvider> _servicesMock = null!;
        private Mock<IAuthenticationService> _authServiceMock = null!;
        private Mock<ITokenProvider> _tokenProviderMock = null!;

        [SetUp]
        public void SetUp()
        {
            _servicesMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            _authServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            _tokenProviderMock = new Mock<ITokenProvider>(MockBehavior.Strict);
        }

        private void SetupAuthService() =>
            _servicesMock.Setup(s => s.GetService(typeof(IAuthenticationService))).Returns(_authServiceMock.Object);

        private (HttpClient Client, StubHandler Stub) CreateClient(params HttpResponseMessage[] responses)
        {
            var stub = new StubHandler(responses);
            var handler = new AuthRefreshHandler(_servicesMock.Object, _tokenProviderMock.Object)
            {
                InnerHandler = stub
            };
            return (new HttpClient(handler), stub);
        }

        [Test]
        public async Task SendAsync_NonUnauthorizedResponse_IsPassedThroughUnchangedAndRefreshIsNeverAttempted()
        {
            var (client, stub) = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/foo");

            var response = await client.SendAsync(request);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(stub.CallCount, Is.EqualTo(1));
            }
            _authServiceMock.Verify(a => a.RefreshAsync(It.IsAny<CancellationToken>()), Times.Never());
            _tokenProviderMock.Verify(t => t.GetTokenAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Test]
        public async Task SendAsync_UnauthorizedThenRefreshSucceeds_RetriesWithNewBearerTokenAndReturnsRetryResponse()
        {
            SetupAuthService();
            _authServiceMock.Setup(a => a.RefreshAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _tokenProviderMock.Setup(t => t.GetTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync("new-token");

            var (client, stub) = CreateClient(
                new HttpResponseMessage(HttpStatusCode.Unauthorized),
                new HttpResponseMessage(HttpStatusCode.OK));

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/foo");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "old-token");

            var response = await client.SendAsync(request);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(stub.CallCount, Is.EqualTo(2));
                Assert.That(stub.CapturedRequests[1].Headers.Authorization?.Scheme, Is.EqualTo("Bearer"));
                Assert.That(stub.CapturedRequests[1].Headers.Authorization?.Parameter, Is.EqualTo("new-token"));
            }
            _authServiceMock.Verify(a => a.RefreshAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Test]
        public async Task SendAsync_UnauthorizedThenRefreshFails_ReturnsOriginalUnauthorizedResponseUnchanged()
        {
            SetupAuthService();
            _authServiceMock.Setup(a => a.RefreshAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var (client, stub) = CreateClient(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/foo");

            var response = await client.SendAsync(request);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
                Assert.That(stub.CallCount, Is.EqualTo(1));
            }
            _tokenProviderMock.Verify(t => t.GetTokenAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Test]
        public async Task SendAsync_UnauthorizedThenRefreshSucceedsButTokenIsNull_ReturnsOriginalUnauthorizedResponseUnchanged()
        {
            SetupAuthService();
            _authServiceMock.Setup(a => a.RefreshAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _tokenProviderMock.Setup(t => t.GetTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);

            var (client, stub) = CreateClient(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/foo");

            var response = await client.SendAsync(request);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
                Assert.That(stub.CallCount, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task SendAsync_UnauthorizedThenRefreshSucceedsButTokenIsEmpty_ReturnsOriginalUnauthorizedResponseUnchanged()
        {
            SetupAuthService();
            _authServiceMock.Setup(a => a.RefreshAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _tokenProviderMock.Setup(t => t.GetTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(string.Empty);

            var (client, stub) = CreateClient(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/foo");

            var response = await client.SendAsync(request);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
                Assert.That(stub.CallCount, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task SendAsync_OnRetry_PreservesCustomHeadersButReplacesAuthorizationHeader()
        {
            SetupAuthService();
            _authServiceMock.Setup(a => a.RefreshAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _tokenProviderMock.Setup(t => t.GetTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync("new-token");

            var (client, stub) = CreateClient(
                new HttpResponseMessage(HttpStatusCode.Unauthorized),
                new HttpResponseMessage(HttpStatusCode.OK));

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/foo");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "old-token");
            request.Headers.Add("X-Custom", "custom-value");

            await client.SendAsync(request);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(stub.CapturedRequests[0].Headers.Authorization?.Parameter, Is.EqualTo("old-token"));
                Assert.That(stub.CapturedRequests[1].Headers.Authorization?.Parameter, Is.EqualTo("new-token"));
                Assert.That(stub.CapturedRequests[1].Headers.GetValues("X-Custom").Single(), Is.EqualTo("custom-value"));
            }
        }
    }
}
