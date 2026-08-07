using System.Net;
using System.Text.Json;
using Common.Http;
using Common.Models;
using Common.Pagination;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Shared.Models;
using RichardSzalay.MockHttp;

namespace RecipeBook.Services.Http.Tests
{
    [TestFixture]
    public class RecipeCategoryServiceTests
    {
        private const string BaseAddress = "https://api.test/";
        private const string RecipeCategoryPath = "api/recipecategory";

        private static JsonSerializerOptions JsonOptions => new(JsonSerializerDefaults.Web);

        private static RecipeCategoryService CreateService(
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
            var logger = Mock.Of<ILogger<RecipeCategoryService>>();

            return new RecipeCategoryService(httpClient, tokenProvider.Object, cache ?? new MemoryCache(new MemoryCacheOptions()), logger);
        }

        // ---------- GetEditAsync ----------
        [Test]
        public async Task GetEditAsync_ReturnsDeserializedModel_AndSendsAuthHeader()
        {
            // Arrange
            const string token = "my-jwt-token";
            var id = Guid.NewGuid();
            var expected = new RecipeCategoryEditModel { Id = id };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{RecipeCategoryPath}/edit*")
                .With(m =>
                {
                    var auth = m.Headers.Authorization;
                    return auth is not null
                           && auth.Scheme == "Bearer"
                           && auth.Parameter == token
                           && m.RequestUri!.Query.Contains($"id={id}");
                })
                .Respond("application/json", JsonSerializer.Serialize(expected, JsonOptions));

            var service = CreateService(mockHttp, token);

            // Act
            var result = await service.GetEditAsync(id);

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

            var paged = new PagedList<RecipeCategoryModel>(
                [
                    new RecipeCategoryModel(),
                    new RecipeCategoryModel()
                ],
                metadata);

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{RecipeCategoryPath}/search*")
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
                .Expect(HttpMethod.Get, $"{BaseAddress}{RecipeCategoryPath}/search*")
                .Respond(HttpStatusCode.InternalServerError);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.SearchAsync());
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task SearchAsync_SecondCall_ReturnsCachedResult_WithoutExtraHttpRequest()
        {
            var paged = new PagedList<RecipeCategoryModel>([new RecipeCategoryModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{RecipeCategoryPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(mockHttp, cache: cache);

            await service.SearchAsync();
            var second = await service.SearchAsync();

            Assert.That(second, Is.Not.Null);
            Assert.That(second!.Items, Has.Count.EqualTo(1));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task AddAsync_InvalidatesCache_NextSearchHitsHttp()
        {
            var paged = new PagedList<RecipeCategoryModel>([new RecipeCategoryModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var addResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{RecipeCategoryPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));
            mockHttp.Expect(HttpMethod.Post, $"{BaseAddress}{RecipeCategoryPath}")
                .Respond("application/json", JsonSerializer.Serialize(addResponse, JsonOptions));
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{RecipeCategoryPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(mockHttp, cache: cache);

            await service.SearchAsync();
            await service.AddAsync(new RecipeCategoryEditModel { Id = Guid.NewGuid() });
            await service.SearchAsync();

            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateAsync_InvalidatesCache_NextSearchHitsHttp()
        {
            var paged = new PagedList<RecipeCategoryModel>([new RecipeCategoryModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var updateResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{RecipeCategoryPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));
            mockHttp.Expect(HttpMethod.Put, $"{BaseAddress}{RecipeCategoryPath}")
                .Respond("application/json", JsonSerializer.Serialize(updateResponse, JsonOptions));
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{RecipeCategoryPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(mockHttp, cache: cache);

            await service.SearchAsync();
            await service.UpdateAsync(new RecipeCategoryEditModel { Id = Guid.NewGuid() });
            await service.SearchAsync();

            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task BulkUpdateAsync_InvalidatesCache_NextSearchHitsHttp()
        {
            var paged = new PagedList<RecipeCategoryModel>([new RecipeCategoryModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var updateResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{RecipeCategoryPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));
            mockHttp.Expect(HttpMethod.Put, $"{BaseAddress}{RecipeCategoryPath}/updateAll")
                .Respond("application/json", JsonSerializer.Serialize(updateResponse, JsonOptions));
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{RecipeCategoryPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(mockHttp, cache: cache);

            await service.SearchAsync();
            await service.UpdateAsync(new List<RecipeCategoryModel> { new() { Id = Guid.NewGuid() } });
            await service.SearchAsync();

            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task DeleteAsync_InvalidatesCache_NextSearchHitsHttp()
        {
            var paged = new PagedList<RecipeCategoryModel>([new RecipeCategoryModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var deleteResponse = new CommandResponse { Succeeded = true };
            var deleteId = Guid.NewGuid();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{RecipeCategoryPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));
            mockHttp.Expect(HttpMethod.Delete, $"{BaseAddress}{RecipeCategoryPath}*")
                .Respond("application/json", JsonSerializer.Serialize(deleteResponse, JsonOptions));
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{RecipeCategoryPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(mockHttp, cache: cache);

            await service.SearchAsync();
            await service.DeleteAsync(deleteId);
            await service.SearchAsync();

            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task SearchAsync_DifferentUsers_SharedCache_DoesNotLeakBetweenUsers()
        {
            var userAResult = new PagedList<RecipeCategoryModel>([new RecipeCategoryModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var userBResult = new PagedList<RecipeCategoryModel>([new RecipeCategoryModel(), new RecipeCategoryModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 2 });

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{RecipeCategoryPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(userAResult, JsonOptions));
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{RecipeCategoryPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(userBResult, JsonOptions));

            // Simulates the Blazor Server scenario: one IMemoryCache instance shared across
            // concurrently logged-in users of the same process.
            var sharedCache = new MemoryCache(new MemoryCacheOptions());
            var serviceForUserA = CreateService(mockHttp, token: CreateJwt("user-a"), cache: sharedCache);
            var serviceForUserB = CreateService(mockHttp, token: CreateJwt("user-b"), cache: sharedCache);

            var resultA = await serviceForUserA.SearchAsync();
            var resultB = await serviceForUserB.SearchAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(resultA!.Items, Has.Count.EqualTo(1));
                Assert.That(resultB!.Items, Has.Count.EqualTo(2));
            }
            mockHttp.VerifyNoOutstandingExpectation();
        }

        private static string CreateJwt(string userId)
        {
            var header = Base64UrlEncode("{\"alg\":\"none\",\"typ\":\"JWT\"}"u8.ToArray());
            var payload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new { sub = userId }));
            return $"{header}.{payload}.signature";
        }

        private static string Base64UrlEncode(byte[] bytes) =>
            Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        // ---------- AddAsync ----------
        [Test]
        public async Task AddAsync_PostsModel_AndReturnsCommandResponse()
        {
            // Arrange
            var model = new RecipeCategoryEditModel { Id = Guid.NewGuid() };
            var expectedResponse = new CommandResponse { Succeeded = true, Message = "ok" };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{RecipeCategoryPath}")
                .With(m =>
                {
                    var body = m.Content!.ReadAsStringAsync().Result;
                    var deserialized = JsonSerializer.Deserialize<RecipeCategoryEditModel>(body, JsonOptions);
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
            var model = new RecipeCategoryEditModel { Id = Guid.NewGuid() };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{RecipeCategoryPath}")
                .Respond(HttpStatusCode.BadRequest);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.AddAsync(model));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- UpdateAsync (single) ----------
        [Test]
        public async Task UpdateAsync_PutsModel_AndReturnsCommandResponse()
        {
            // Arrange
            var model = new RecipeCategoryEditModel { Id = Guid.NewGuid() };
            var expectedResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{RecipeCategoryPath}")
                .With(m =>
                {
                    var body = m.Content!.ReadAsStringAsync().Result;
                    var deserialized = JsonSerializer.Deserialize<RecipeCategoryEditModel>(body, JsonOptions);
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
            var model = new RecipeCategoryEditModel { Id = Guid.NewGuid() };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{RecipeCategoryPath}")
                .Respond(HttpStatusCode.BadRequest);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.UpdateAsync(model));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- UpdateAsync (bulk) ----------
        [Test]
        public async Task BulkUpdateAsync_PutsModels_AndReturnsCommandResponse()
        {
            // Arrange
            var models = new List<RecipeCategoryModel>
            {
                new() { Id = Guid.NewGuid() },
                new() { Id = Guid.NewGuid() }
            };

            var expectedResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{RecipeCategoryPath}/updateAll")
                .With(m =>
                {
                    var body = m.Content!.ReadAsStringAsync().Result;
                    var deserialized = JsonSerializer.Deserialize<List<RecipeCategoryModel>>(body, JsonOptions);
                    return deserialized is not null && deserialized.Count == models.Count;
                })
                .Respond("application/json", JsonSerializer.Serialize(expectedResponse, JsonOptions));

            var service = CreateService(mockHttp);

            // Act
            var result = await service.UpdateAsync(models);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void BulkUpdateAsync_Throws_OnNonSuccessStatusCode()
        {
            // Arrange
            var models = new List<RecipeCategoryModel> { new() { Id = Guid.NewGuid() }, new() { Id = Guid.NewGuid() } };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{RecipeCategoryPath}/updateAll")
                .Respond(HttpStatusCode.BadRequest);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.UpdateAsync(models));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- DeleteAsync ----------
        [Test]
        public async Task DeleteAsync_SendsDeleteWithId_AndReturnsCommandResponse()
        {
            // Arrange
            var id = Guid.NewGuid();
            var expectedResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Delete, $"{BaseAddress}{RecipeCategoryPath}*")
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
            var id = Guid.NewGuid();

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Delete, $"{BaseAddress}{RecipeCategoryPath}*")
                .With(m => m.RequestUri!.Query.Contains($"id={id}"))
                .Respond(HttpStatusCode.NotFound);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.DeleteAsync(id));
            mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}
