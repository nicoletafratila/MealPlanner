using System.Net;
using System.Text.Json;
using Common.Http;
using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;

namespace MealPlanner.Services.Http.Tests
{
    [TestFixture]
    public class ShopServiceTests
    {
        private const string BaseAddress = "https://api.test/";
        private const string ShopPath = "api/shop";

        private static JsonSerializerOptions JsonOptions => new(JsonSerializerDefaults.Web);

        private static ShopService CreateService(
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
            var logger = Mock.Of<ILogger<ShopService>>();

            return new ShopService(httpClient, tokenProvider.Object, cache ?? new MemoryCache(new MemoryCacheOptions()), logger);
        }

        // ---------- GetEditAsync ----------
        [Test]
        public async Task GetEditAsync_ReturnsDeserializedModel_AndSendsAuthHeader()
        {
            // Arrange
            const string token = "my-jwt-token";
            var id = Guid.NewGuid();
            var expected = new ShopEditModel { Id = id };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{ShopPath}/edit*")
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

            var paged = new PagedList<ShopModel>(
                [
                    new ShopModel(),
                    new ShopModel()
                ],
                metadata);

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{ShopPath}/search*")
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
                .Expect(HttpMethod.Get, $"{BaseAddress}{ShopPath}/search*")
                .Respond(HttpStatusCode.InternalServerError);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.SearchAsync());
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task SearchAsync_SecondCall_ReturnsCachedResult_WithoutExtraHttpRequest()
        {
            var paged = new PagedList<ShopModel>([new ShopModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ShopPath}/search*")
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
            var paged = new PagedList<ShopModel>([new ShopModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var addResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ShopPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));
            mockHttp.Expect(HttpMethod.Post, $"{BaseAddress}{ShopPath}")
                .Respond("application/json", JsonSerializer.Serialize(addResponse, JsonOptions));
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ShopPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(mockHttp, cache: cache);

            await service.SearchAsync();
            await service.AddAsync(new ShopEditModel { Id = Guid.NewGuid() });
            await service.SearchAsync();

            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateAsync_InvalidatesCache_NextSearchHitsHttp()
        {
            var paged = new PagedList<ShopModel>([new ShopModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var updateResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ShopPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));
            mockHttp.Expect(HttpMethod.Put, $"{BaseAddress}{ShopPath}")
                .Respond("application/json", JsonSerializer.Serialize(updateResponse, JsonOptions));
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ShopPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(mockHttp, cache: cache);

            await service.SearchAsync();
            await service.UpdateAsync(new ShopEditModel { Id = Guid.NewGuid() });
            await service.SearchAsync();

            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task DeleteAsync_InvalidatesCache_NextSearchHitsHttp()
        {
            var paged = new PagedList<ShopModel>([new ShopModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var deleteResponse = new CommandResponse { Succeeded = true };
            var deleteId = Guid.NewGuid();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ShopPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));
            mockHttp.Expect(HttpMethod.Delete, $"{BaseAddress}{ShopPath}*")
                .Respond("application/json", JsonSerializer.Serialize(deleteResponse, JsonOptions));
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ShopPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(mockHttp, cache: cache);

            await service.SearchAsync();
            await service.DeleteAsync(deleteId);
            await service.SearchAsync();

            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task SearchAsync_DifferentFilters_SamePageAndSorting_DoesNotReuseOtherFiltersCachedResult()
        {
            var pagedA = new PagedList<ShopModel>([new ShopModel(Guid.NewGuid(), "Lidl")], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var pagedB = new PagedList<ShopModel>([new ShopModel(Guid.NewGuid(), "Kaufland")], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ShopPath}/search*")
                .With(m => Uri.UnescapeDataString(m.RequestUri!.Query).Contains("\"lidl\""))
                .Respond("application/json", JsonSerializer.Serialize(pagedA, JsonOptions));
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ShopPath}/search*")
                .With(m => Uri.UnescapeDataString(m.RequestUri!.Query).Contains("\"kaufland\""))
                .Respond("application/json", JsonSerializer.Serialize(pagedB, JsonOptions));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(mockHttp, cache: cache);

            var resultA = await service.SearchAsync(new QueryParameters<ShopModel>
            {
                Filters = [new FilterItem("Name", "lidl", FilterOperator.Contains)]
            });
            var resultB = await service.SearchAsync(new QueryParameters<ShopModel>
            {
                Filters = [new FilterItem("Name", "kaufland", FilterOperator.Contains)]
            });

            using (Assert.EnterMultipleScope())
            {
                Assert.That(resultA!.Items[0].Name, Is.EqualTo("Lidl"));
                Assert.That(resultB!.Items[0].Name, Is.EqualTo("Kaufland"));
            }
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task SearchAsync_DifferentUsers_SharedCache_DoesNotLeakBetweenUsers()
        {
            var userAShops = new PagedList<ShopModel>([new ShopModel(Guid.NewGuid(), "UserA-Shop")], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var userBShops = new PagedList<ShopModel>([new ShopModel(Guid.NewGuid(), "UserB-Shop")], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ShopPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(userAShops, JsonOptions));
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ShopPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(userBShops, JsonOptions));

            // Simulates the Blazor Server scenario: one IMemoryCache instance shared across
            // concurrently logged-in users of the same process.
            var sharedCache = new MemoryCache(new MemoryCacheOptions());
            var serviceForUserA = CreateService(mockHttp, token: CreateJwt("user-a"), cache: sharedCache);
            var serviceForUserB = CreateService(mockHttp, token: CreateJwt("user-b"), cache: sharedCache);

            var resultA = await serviceForUserA.SearchAsync();
            var resultB = await serviceForUserB.SearchAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(resultA!.Items[0].Name, Is.EqualTo("UserA-Shop"));
                Assert.That(resultB!.Items[0].Name, Is.EqualTo("UserB-Shop"));
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
            var model = new ShopEditModel { Id = Guid.NewGuid() };
            var expectedResponse = new CommandResponse { Succeeded = true, Message = "ok" };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{ShopPath}")
                .With(m =>
                {
                    var body = m.Content!.ReadAsStringAsync().Result;
                    var deserialized = JsonSerializer.Deserialize<ShopEditModel>(body, JsonOptions);
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
            var model = new ShopEditModel { Id = Guid.NewGuid() };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{ShopPath}")
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
            var model = new ShopEditModel { Id = Guid.NewGuid() };
            var expectedResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{ShopPath}")
                .With(m =>
                {
                    var body = m.Content!.ReadAsStringAsync().Result;
                    var deserialized = JsonSerializer.Deserialize<ShopEditModel>(body, JsonOptions);
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
            var model = new ShopEditModel { Id = Guid.NewGuid() };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{ShopPath}")
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
            var id = Guid.NewGuid();
            var expectedResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Delete, $"{BaseAddress}{ShopPath}*")
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
                .Expect(HttpMethod.Delete, $"{BaseAddress}{ShopPath}*")
                .With(m => m.RequestUri!.Query.Contains($"id={id}"))
                .Respond(HttpStatusCode.NotFound);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.DeleteAsync(id));
            mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}
