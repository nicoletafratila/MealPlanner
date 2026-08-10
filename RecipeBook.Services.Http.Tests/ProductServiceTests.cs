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
    public class ProductServiceTests
    {
        private const string BaseAddress = "https://api.test/";
        private const string ProductPath = "api/product";

        private static JsonSerializerOptions JsonOptions => new(JsonSerializerDefaults.Web);

        private static ProductService CreateService(
            MockHttpMessageHandler mockHttp,
            string token = "test-token",
            IMemoryCache? cache = null)
        {
            var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseAddress) };
            var tokenProvider = new Mock<ITokenProvider>();
            tokenProvider.Setup(t => t.GetTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(token);
            var logger = Mock.Of<ILogger<ProductService>>();
            return new ProductService(httpClient, tokenProvider.Object, cache ?? new MemoryCache(new MemoryCacheOptions()), logger);
        }

        // ---------- GetEditAsync ----------
        [Test]
        public async Task GetEditAsync_ReturnsDeserializedModel_AndSendsAuthHeader()
        {
            const string token = "my-jwt-token";
            var id = Guid.NewGuid();
            var expected = new ProductEditModel { Id = id };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{ProductPath}/edit*")
                .With(m =>
                {
                    var auth = m.Headers.Authorization;
                    return auth is not null && auth.Scheme == "Bearer" && auth.Parameter == token
                           && m.RequestUri!.Query.Contains($"id={id}");
                })
                .Respond("application/json", JsonSerializer.Serialize(expected, JsonOptions));

            var service = CreateService(mockHttp, token);
            var result = await service.GetEditAsync(id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(expected.Id));
            mockHttp.VerifyNoOutstandingExpectation();
            mockHttp.VerifyNoOutstandingRequest();
        }

        // ---------- SearchAsync ----------
        [Test]
        public async Task SearchAsync_DeserializesPagedList_OnSuccess()
        {
            var paged = new PagedList<ProductModel>([new ProductModel(), new ProductModel()],
                new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 2 });

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ProductPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var service = CreateService(mockHttp);
            var result = await service.SearchAsync();

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
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ProductPath}/search*").Respond(HttpStatusCode.InternalServerError);
            var service = CreateService(mockHttp);
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.SearchAsync());
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task SearchAsync_SecondCall_ReturnsCachedResult_WithoutExtraHttpRequest()
        {
            var paged = new PagedList<ProductModel>([new ProductModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ProductPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(mockHttp, cache: cache);

            var first = await service.SearchAsync();
            var second = await service.SearchAsync();

            Assert.That(second, Is.Not.Null);
            Assert.That(second!.Items, Has.Count.EqualTo(1));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task AddAsync_InvalidatesCache_NextSearchHitsHttp()
        {
            var paged = new PagedList<ProductModel>([new ProductModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var addResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ProductPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));
            mockHttp.Expect(HttpMethod.Post, $"{BaseAddress}{ProductPath}")
                .Respond("application/json", JsonSerializer.Serialize(addResponse, JsonOptions));
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ProductPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(mockHttp, cache: cache);

            await service.SearchAsync();
            await service.AddAsync(new ProductEditModel { Id = Guid.NewGuid() });
            await service.SearchAsync();

            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateAsync_InvalidatesCache_NextSearchHitsHttp()
        {
            var paged = new PagedList<ProductModel>([new ProductModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var updateResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ProductPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));
            mockHttp.Expect(HttpMethod.Put, $"{BaseAddress}{ProductPath}")
                .Respond("application/json", JsonSerializer.Serialize(updateResponse, JsonOptions));
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ProductPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(mockHttp, cache: cache);

            await service.SearchAsync();
            await service.UpdateAsync(new ProductEditModel { Id = Guid.NewGuid() });
            await service.SearchAsync();

            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task DeleteAsync_InvalidatesCache_NextSearchHitsHttp()
        {
            var paged = new PagedList<ProductModel>([new ProductModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var deleteResponse = new CommandResponse { Succeeded = true };
            var deleteId = Guid.NewGuid();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ProductPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));
            mockHttp.Expect(HttpMethod.Delete, $"{BaseAddress}{ProductPath}*")
                .Respond("application/json", JsonSerializer.Serialize(deleteResponse, JsonOptions));
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ProductPath}/search*")
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
            var pagedMilk = new PagedList<ProductModel>([new ProductModel(Guid.NewGuid(), "Milk")], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var pagedBread = new PagedList<ProductModel>([new ProductModel(Guid.NewGuid(), "Bread")], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ProductPath}/search*")
                .With(m => Uri.UnescapeDataString(m.RequestUri!.Query).Contains("\"milk\""))
                .Respond("application/json", JsonSerializer.Serialize(pagedMilk, JsonOptions));
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ProductPath}/search*")
                .With(m => Uri.UnescapeDataString(m.RequestUri!.Query).Contains("\"bread\""))
                .Respond("application/json", JsonSerializer.Serialize(pagedBread, JsonOptions));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(mockHttp, cache: cache);

            var milkResult = await service.SearchAsync(new QueryParameters<ProductModel>
            {
                Filters = [new FilterItem("Name", "milk", FilterOperator.Contains)]
            });
            var breadResult = await service.SearchAsync(new QueryParameters<ProductModel>
            {
                Filters = [new FilterItem("Name", "bread", FilterOperator.Contains)]
            });

            using (Assert.EnterMultipleScope())
            {
                Assert.That(milkResult!.Items[0].Name, Is.EqualTo("Milk"));
                Assert.That(breadResult!.Items[0].Name, Is.EqualTo("Bread"));
            }
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task SearchAsync_DifferentUsers_SharedCache_DoesNotLeakBetweenUsers()
        {
            var userAResult = new PagedList<ProductModel>([new ProductModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var userBResult = new PagedList<ProductModel>([new ProductModel(), new ProductModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 2 });

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ProductPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(userAResult, JsonOptions));
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ProductPath}/search*")
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

        [Test]
        public async Task SearchAsync_ThumbnailOnlyTrue_IncludesThumbnailOnlyInRequestQueryString()
        {
            var paged = new PagedList<ProductModel>([new ProductModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{ProductPath}/search*")
                .With(m => m.RequestUri!.Query.Contains("thumbnailOnly=true"))
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var service = CreateService(mockHttp);

            var result = await service.SearchAsync(thumbnailOnly: true);

            Assert.That(result, Is.Not.Null);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task SearchAsync_ThumbnailOnlyFalse_OmitsThumbnailOnlyFromRequestQueryString()
        {
            var paged = new PagedList<ProductModel>([new ProductModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{ProductPath}/search*")
                .With(m => !m.RequestUri!.Query.Contains("thumbnailOnly"))
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var service = CreateService(mockHttp);

            var result = await service.SearchAsync();

            Assert.That(result, Is.Not.Null);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task SearchAsync_SameQueryParametersDifferentThumbnailOnly_DoesNotShareCache()
        {
            var withThumbnails = new PagedList<ProductModel>([new ProductModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });
            var withFullImages = new PagedList<ProductModel>([new ProductModel(), new ProductModel()], new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 2 });

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ProductPath}/search*")
                .With(m => m.RequestUri!.Query.Contains("thumbnailOnly=true"))
                .Respond("application/json", JsonSerializer.Serialize(withThumbnails, JsonOptions));
            mockHttp.Expect(HttpMethod.Get, $"{BaseAddress}{ProductPath}/search*")
                .With(m => !m.RequestUri!.Query.Contains("thumbnailOnly"))
                .Respond("application/json", JsonSerializer.Serialize(withFullImages, JsonOptions));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(mockHttp, cache: cache);

            var thumbnailResult = await service.SearchAsync(thumbnailOnly: true);
            var fullImageResult = await service.SearchAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(thumbnailResult!.Items, Has.Count.EqualTo(1));
                Assert.That(fullImageResult!.Items, Has.Count.EqualTo(2));
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
            var model = new ProductEditModel { Id = Guid.NewGuid() };
            var expectedResponse = new CommandResponse { Succeeded = true, Message = "ok" };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{ProductPath}")
                .With(m =>
                {
                    var deserialized = JsonSerializer.Deserialize<ProductEditModel>(m.Content!.ReadAsStringAsync().Result, JsonOptions);
                    return deserialized is not null && deserialized.Id == model.Id;
                })
                .Respond("application/json", JsonSerializer.Serialize(expectedResponse, JsonOptions));

            var service = CreateService(mockHttp);
            var result = await service.AddAsync(model);

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
            var model = new ProductEditModel { Id = Guid.NewGuid() };
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Post, $"{BaseAddress}{ProductPath}").Respond(HttpStatusCode.BadRequest);
            var service = CreateService(mockHttp);
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.AddAsync(model));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- UpdateAsync ----------
        [Test]
        public async Task UpdateAsync_PutsModel_AndReturnsCommandResponse()
        {
            var model = new ProductEditModel { Id = Guid.NewGuid() };
            var expectedResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{ProductPath}")
                .With(m =>
                {
                    var deserialized = JsonSerializer.Deserialize<ProductEditModel>(m.Content!.ReadAsStringAsync().Result, JsonOptions);
                    return deserialized is not null && deserialized.Id == model.Id;
                })
                .Respond("application/json", JsonSerializer.Serialize(expectedResponse, JsonOptions));

            var service = CreateService(mockHttp);
            var result = await service.UpdateAsync(model);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void UpdateAsync_Throws_OnNonSuccessStatusCode()
        {
            var model = new ProductEditModel { Id = Guid.NewGuid() };
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Put, $"{BaseAddress}{ProductPath}").Respond(HttpStatusCode.BadRequest);
            var service = CreateService(mockHttp);
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.UpdateAsync(model));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- DeleteAsync ----------
        [Test]
        public async Task DeleteAsync_SendsDeleteWithId_AndReturnsCommandResponse()
        {
            var id = Guid.NewGuid();
            var expectedResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Delete, $"{BaseAddress}{ProductPath}*")
                .With(m => m.RequestUri!.Query.Contains($"id={id}"))
                .Respond("application/json", JsonSerializer.Serialize(expectedResponse, JsonOptions));

            var service = CreateService(mockHttp);
            var result = await service.DeleteAsync(id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void DeleteAsync_Throws_OnNonSuccessStatusCode()
        {
            var id = Guid.NewGuid();
            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Delete, $"{BaseAddress}{ProductPath}*")
                .With(m => m.RequestUri!.Query.Contains($"id={id}"))
                .Respond(HttpStatusCode.NotFound);
            var service = CreateService(mockHttp);
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.DeleteAsync(id));
            mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}
