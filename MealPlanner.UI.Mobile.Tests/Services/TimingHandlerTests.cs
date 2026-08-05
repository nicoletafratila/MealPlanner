using System.Net;
using MealPlanner.UI.Mobile.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace MealPlanner.UI.Mobile.Tests.Services
{
    [TestFixture]
    public class TimingHandlerTests
    {
        private sealed class StubHandler(HttpResponseMessage response) : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
                Task.FromResult(response);
        }

        private Mock<ILogger<TimingHandler>> _loggerMock = null!;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<TimingHandler>>(MockBehavior.Loose);
        }

        private (HttpClient Client, StubHandler Stub) CreateClient(HttpResponseMessage response)
        {
            var stub = new StubHandler(response);
            var handler = new TimingHandler(_loggerMock.Object) { InnerHandler = stub };
            return (new HttpClient(handler), stub);
        }

        [Test]
        public async Task SendAsync_ForwardsRequestToInnerHandler_AndReturnsItsResponseUnchanged()
        {
            var (client, _) = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));

            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://api.test/foo"));

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task SendAsync_LogsElapsedTimeForTheRequest()
        {
            var (client, _) = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));

            await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://api.test/foo"));

            _loggerMock.Verify(
                l => l.Log(
                    It.Is<LogLevel>(ll => ll == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public void SendAsync_InnerHandlerThrows_LogsElapsedTime_AndPropagatesException()
        {
            var stub = new ThrowingStubHandler();
            var handler = new TimingHandler(_loggerMock.Object) { InnerHandler = stub };
            var client = new HttpClient(handler);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://api.test/foo")));

            _loggerMock.Verify(
                l => l.Log(
                    It.Is<LogLevel>(ll => ll == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private sealed class ThrowingStubHandler : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
                throw new InvalidOperationException("Simulated network failure");
        }
    }
}
