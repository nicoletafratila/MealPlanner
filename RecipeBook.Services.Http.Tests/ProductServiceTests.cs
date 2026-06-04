using System.Net;
using System.Text.Json;
using Common.Http;
using RecipeBook.Services.Http;
using Common.Models;
using Common.Pagination;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Shared.Constants;
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

        private static ProductService CreateService(MockHttpMessageHandler mockHttp, string token = "test-token")
        {
            var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseAddress) };
            var tokenProvider = new Mock<ITokenProvider>();
            tokenProvider.Setup(t => t.GetTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(token);
            var logger = Mock.Of<ILogger<ProductService>>();
            return new ProductService(httpClient, tokenProvider.Object, logger);
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
