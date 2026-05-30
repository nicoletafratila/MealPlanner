using System.Net;
using System.Text.Json;
using Common.Http;
using RecipeBook.Services.Http;
using Common.Models;
using Common.Pagination;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Shared.Constants;
using RecipeBook.Shared.Models;
using RichardSzalay.MockHttp;

namespace RecipeBook.Services.Http.Tests
{
    [TestFixture]
    public class UnitServiceTests
    {
        private const string BaseAddress = "https://api.test/";
        private const string UnitPath = "api/unit";

        private static JsonSerializerOptions JsonOptions => new(JsonSerializerDefaults.Web);

        private static UnitService CreateService(
            MockHttpMessageHandler mockHttp,
            string token = "test-token",
            IMemoryCache? cache = null)
        {
            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(BaseAddress)
            };

            var tokenProvider = new Mock<ITokenProvider>();
            tokenProvider
                .Setup(t => t.GetTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);
            var logger = Mock.Of<ILogger<UnitService>>();

            return new UnitService(httpClient, tokenProvider.Object, cache ?? new MemoryCache(new MemoryCacheOptions()), logger);
        }

        // ---------- GetEditAsync ----------
        [Test]
        public async Task GetEditAsync_ReturnsDeserializedModel_AndSendsAuthHeader()
        {
            // Arrange
            const string token = "my-jwt-token";
            var unitId = 42;
            var expected = new UnitEditModel { Id = unitId };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{UnitPath}/edit*")
                .With(m =>
                {
                    var auth = m.Headers.Authorization;
                    return auth is not null
                           && auth.Scheme == "Bearer"
                           && auth.Parameter == token
                           && m.RequestUri!.Query.Contains($"id={unitId}");
                })
                .Respond("application/json", JsonSerializer.Serialize(expected, JsonOptions));

            var service = CreateService(mockHttp, token);

            // Act
            var result = await service.GetEditAsync(unitId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(expected.Id));
            mockHttp.VerifyNoOutstandingExpectation();
            mockHttp.VerifyNoOutstandingRequest();
        }

        // ---------- SearchAsync ----------
        [Test]
        public async Task SearchAsync_DeserializesPagedList_OnSuccess()
        {
            // Arrange
            var metadata = new Metadata
            {
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 2
            };

            var paged = new PagedList<UnitModel>(
                [
                    new UnitModel(),
                    new UnitModel()
                ],
                metadata);

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{UnitPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var service = CreateService(mockHttp);

            // Act
            var result = await service.SearchAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Items, Has.Count.EqualTo(2));
                Assert.That(result.Metadata.PageNumber, Is.EqualTo(1));
            }
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void SearchAsync_Throws_OnNonSuccessStatusCode()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{UnitPath}/search*")
                .Respond(HttpStatusCode.InternalServerError);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.SearchAsync());
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task SearchAsync_SecondCall_ReturnsCachedResult_WithoutExtraHttpRequest()
        {
            // Arrange
            var paged = new PagedList<UnitModel>([new UnitModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{UnitPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(mockHttp, cache: cache);

            // Act
            var first = await service.SearchAsync();
            var second = await service.SearchAsync();

            // Assert — only one HTTP request was made
            Assert.That(second, Is.Not.Null);
            Assert.That(second!.Items, Has.Count.EqualTo(1));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task AddAsync_InvalidatesCache_NextSearchHitsHttp()
        {
            // Arrange
            var paged = new PagedList<UnitModel>([new UnitModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var addResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{UnitPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{UnitPath}")
                .Respond("application/json", JsonSerializer.Serialize(addResponse, JsonOptions));

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{UnitPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(mockHttp, cache: cache);

            // Act — populate cache, then invalidate via Add, then search again
            await service.SearchAsync();
            await service.AddAsync(new UnitEditModel { Id = 1 });
            await service.SearchAsync();

            // Assert — all three HTTP calls were made
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateAsync_InvalidatesCache_NextSearchHitsHttp()
        {
            // Arrange
            var paged = new PagedList<UnitModel>([new UnitModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var updateResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{UnitPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{UnitPath}")
                .Respond("application/json", JsonSerializer.Serialize(updateResponse, JsonOptions));

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{UnitPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(mockHttp, cache: cache);

            // Act
            await service.SearchAsync();
            await service.UpdateAsync(new UnitEditModel { Id = 1 });
            await service.SearchAsync();

            // Assert
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task DeleteAsync_InvalidatesCache_NextSearchHitsHttp()
        {
            // Arrange
            var paged = new PagedList<UnitModel>([new UnitModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var deleteResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{UnitPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            mockHttp
                .Expect(HttpMethod.Delete, $"{BaseAddress}{UnitPath}*")
                .Respond("application/json", JsonSerializer.Serialize(deleteResponse, JsonOptions));

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{UnitPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(mockHttp, cache: cache);

            // Act
            await service.SearchAsync();
            await service.DeleteAsync(1);
            await service.SearchAsync();

            // Assert
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- AddAsync ----------
        [Test]
        public async Task AddAsync_PostsModel_AndReturnsCommandResponse()
        {
            // Arrange
            var model = new UnitEditModel { Id = 1 };
            var expectedResponse = new CommandResponse { Succeeded = true, Message = "ok" };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{UnitPath}")
                .With(m =>
                {
                    var body = m.Content!.ReadAsStringAsync().Result;
                    var deserialized = JsonSerializer.Deserialize<UnitEditModel>(body, JsonOptions);
                    return deserialized is not null && deserialized.Id == model.Id;
                })
                .Respond("application/json", JsonSerializer.Serialize(expectedResponse, JsonOptions));

            var service = CreateService(mockHttp);

            // Act
            var result = await service.AddAsync(model);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.True);
                Assert.That(result.Message, Is.EqualTo("ok"));
            }
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void AddAsync_Throws_OnNonSuccessStatusCode()
        {
            // Arrange
            var model = new UnitEditModel { Id = 1 };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{UnitPath}")
                .Respond(HttpStatusCode.BadRequest);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.AddAsync(model));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- UpdateAsync ----------
        [Test]
        public async Task UpdateAsync_PutsModel_AndReturnsCommandResponse()
        {
            // Arrange
            var model = new UnitEditModel { Id = 2 };
            var expectedResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{UnitPath}")
                .With(m =>
                {
                    var body = m.Content!.ReadAsStringAsync().Result;
                    var deserialized = JsonSerializer.Deserialize<UnitEditModel>(body, JsonOptions);
                    return deserialized is not null && deserialized.Id == model.Id;
                })
                .Respond("application/json", JsonSerializer.Serialize(expectedResponse, JsonOptions));

            var service = CreateService(mockHttp);

            // Act
            var result = await service.UpdateAsync(model);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void UpdateAsync_Throws_OnNonSuccessStatusCode()
        {
            // Arrange
            var model = new UnitEditModel { Id = 2 };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{UnitPath}")
                .Respond(HttpStatusCode.BadRequest);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.UpdateAsync(model));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- DeleteAsync ----------
        [Test]
        public async Task DeleteAsync_SendsDeleteWithId_AndReturnsCommandResponse()
        {
            // Arrange
            var id = 7;
            var expectedResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Delete, $"{BaseAddress}{UnitPath}*")
                .With(m => m.RequestUri!.Query.Contains($"id={id}"))
                .Respond("application/json", JsonSerializer.Serialize(expectedResponse, JsonOptions));

            var service = CreateService(mockHttp);

            // Act
            var result = await service.DeleteAsync(id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void DeleteAsync_Throws_OnNonSuccessStatusCode()
        {
            // Arrange
            var id = 7;

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Delete, $"{BaseAddress}{UnitPath}*")
                .With(m => m.RequestUri!.Query.Contains($"id={id}"))
                .Respond(HttpStatusCode.NotFound);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.DeleteAsync(id));
            mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}
