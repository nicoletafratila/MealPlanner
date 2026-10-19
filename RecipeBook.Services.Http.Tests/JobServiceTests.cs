using System.Net;
using System.Text.Json;
using Common.Http;
using Common.Pagination;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Shared.Models;
using RichardSzalay.MockHttp;

namespace RecipeBook.Services.Http.Tests
{
    [TestFixture]
    public class JobServiceTests
    {
        private const string BaseAddress = "https://api.test/";
        private const string JobPath = "api/job";

        private static JsonSerializerOptions JsonOptions => new(JsonSerializerDefaults.Web);

        private static JobService CreateService(MockHttpMessageHandler mockHttp, string token = "test-token")
        {
            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(BaseAddress)
            };

            var tokenProvider = new Mock<ITokenProvider>();
            tokenProvider
                .Setup(t => t.GetTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);
            var logger = Mock.Of<ILogger<JobService>>();

            return new JobService(httpClient, tokenProvider.Object, logger);
        }

        // ---------- GetStatusAsync ----------
        [Test]
        public async Task GetStatusAsync_ReturnsDeserializedModel()
        {
            var jobId = Guid.NewGuid();
            var expected = new JobStatusModel { JobId = jobId, PercentComplete = 42 };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{JobPath}/{jobId}/status")
                .Respond("application/json", JsonSerializer.Serialize(expected, JsonOptions));

            var service = CreateService(mockHttp);

            var result = await service.GetStatusAsync(jobId);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.JobId, Is.EqualTo(jobId));
                Assert.That(result.PercentComplete, Is.EqualTo(42));
            }
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetStatusAsync_OnError_ReturnsNull()
        {
            var jobId = Guid.NewGuid();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{JobPath}/{jobId}/status")
                .Respond(HttpStatusCode.NotFound);

            var service = CreateService(mockHttp);

            var result = await service.GetStatusAsync(jobId);

            Assert.That(result, Is.Null);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- SearchAsync ----------
        [Test]
        public async Task SearchAsync_ReturnsDeserializedPagedList()
        {
            var paged = new PagedList<JobModel>([new JobModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{JobPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var service = CreateService(mockHttp);

            var result = await service.SearchAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Items, Has.Count.EqualTo(1));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task SearchAsync_OnError_ReturnsNull()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{JobPath}/search*")
                .Respond(HttpStatusCode.InternalServerError);

            var service = CreateService(mockHttp);

            var result = await service.SearchAsync();

            Assert.That(result, Is.Null);
            mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}
