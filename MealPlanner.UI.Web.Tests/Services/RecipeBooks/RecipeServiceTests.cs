using System.Net;
using System.Text.Json;
using Blazored.SessionStorage;
using Common.Api;
using Common.Constants;
using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Shared.Models;
using RichardSzalay.MockHttp;

namespace MealPlanner.UI.Web.Tests.Services.RecipeBooks
{
    [TestFixture]
    public class RecipeServiceTests
    {
        private const string AuthTokenKey = Common.Constants.MealPlanner.AuthToken;
        private const string BaseAddress = "https://api.test/";
        private const string RecipePath = "api/recipe";

        private static JsonSerializerOptions JsonOptions => new(JsonSerializerDefaults.Web);

        private static RecipeBookApiConfig CreateConfig()
        {
            var configuration = new ConfigurationBuilder().Build();
            var cfg = new RecipeBookApiConfig(configuration)
            {
                BaseUrl = new Uri(BaseAddress),
                Controllers = new Dictionary<string, string>
                {
                    [RecipeBookControllers.Recipe] = RecipePath
                }
            };

            return cfg;
        }

        private static RecipeService CreateService(
            MockHttpMessageHandler mockHttp,
            string token = "test-token")
        {
            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(BaseAddress)
            };

            var sessionStorage = new Mock<ISessionStorageService>();
            sessionStorage
                .Setup(s => s.GetItemAsync<string?>(AuthTokenKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            var tokenProvider = new TokenProvider(sessionStorage.Object);
            var config = CreateConfig();
            var logger = Mock.Of<ILogger<RecipeService>>();

            return new RecipeService(httpClient, tokenProvider, config, logger);
        }

        // ---------- GetByIdAsync ----------
        [Test]
        public async Task GetByIdAsync_ReturnsDeserializedModel_AndSendsAuthHeader()
        {
            // Arrange
            const string token = "my-jwt-token";
            var recipeId = 42;
            var expected = new RecipeModel { Id = recipeId };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{RecipePath}*")
                .With(m =>
                {
                    var auth = m.Headers.Authorization;
                    return auth is not null
                           && auth.Scheme == JwtBearerDefaults.AuthenticationScheme
                           && auth.Parameter == token
                           && m.RequestUri!.Query.Contains($"id={recipeId}");
                })
                .Respond("application/json", JsonSerializer.Serialize(expected, JsonOptions));

            var service = CreateService(mockHttp, token);

            // Act
            var result = await service.GetByIdAsync(recipeId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(expected.Id));
            mockHttp.VerifyNoOutstandingExpectation();
            mockHttp.VerifyNoOutstandingRequest();
        }

        // ---------- GetEditAsync ----------
        [Test]
        public async Task GetEditAsync_ReturnsDeserializedModel_AndSendsAuthHeader()
        {
            // Arrange
            const string token = "my-jwt-token";
            var recipeId = 42;
            var expected = new RecipeEditModel { Id = recipeId };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{RecipePath}/edit*")
                .With(m =>
                {
                    var auth = m.Headers.Authorization;
                    return auth is not null
                           && auth.Scheme == JwtBearerDefaults.AuthenticationScheme
                           && auth.Parameter == token
                           && m.RequestUri!.Query.Contains($"id={recipeId}");
                })
                .Respond("application/json", JsonSerializer.Serialize(expected, JsonOptions));

            var service = CreateService(mockHttp, token);

            // Act
            var result = await service.GetEditAsync(recipeId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(expected.Id));
            mockHttp.VerifyNoOutstandingExpectation();
            mockHttp.VerifyNoOutstandingRequest();
        }

        // ---------- GetShoppingListProductsAsync ----------
        [Test]
        public async Task GetShoppingListProductsAsync_ReturnsList()
        {
            // Arrange
            var recipeId = 10;
            var shopId = 5;

            var expected = new List<ShoppingListProductEditModel>
            {
                new ShoppingListProductEditModel(),
                new ShoppingListProductEditModel()
            };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{RecipePath}/shoppingListProducts*")
                .With(m =>
                {
                    var q = m.RequestUri!.Query;
                    return q.Contains($"recipeId={recipeId}") && q.Contains($"shopId={shopId}");
                })
                .Respond("application/json", JsonSerializer.Serialize(expected, JsonOptions));

            var service = CreateService(mockHttp);

            // Act
            var result = await service.GetShoppingListProductsAsync(recipeId, shopId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Count, Is.EqualTo(expected.Count));
            mockHttp.VerifyNoOutstandingExpectation();
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

            var paged = new PagedList<RecipeModel>(
                new[]
                {
                    new RecipeModel(),
                    new RecipeModel()
                },
                metadata);

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{RecipePath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var service = CreateService(mockHttp);

            // Act
            var result = await service.SearchAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Items.Count, Is.EqualTo(2));
            Assert.That(result.Metadata.PageNumber, Is.EqualTo(1));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void SearchAsync_Throws_OnNonSuccessStatusCode()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{RecipePath}/search*")
                .Respond(HttpStatusCode.InternalServerError);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.SearchAsync());
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- AddAsync ----------
        [Test]
        public async Task AddAsync_PostsModel_AndReturnsCommandResponse()
        {
            // Arrange
            var model = new RecipeEditModel { Id = 1 };
            var expectedResponse = new CommandResponse { Succeeded = true, Message = "ok" };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{RecipePath}")
                .With(m =>
                {
                    var body = m.Content!.ReadAsStringAsync().Result;
                    var deserialized = JsonSerializer.Deserialize<RecipeEditModel>(body, JsonOptions);
                    return deserialized is not null && deserialized.Id == model.Id;
                })
                .Respond("application/json", JsonSerializer.Serialize(expectedResponse, JsonOptions));

            var service = CreateService(mockHttp);

            // Act
            var result = await service.AddAsync(model);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);
            Assert.That(result.Message, Is.EqualTo("ok"));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void AddAsync_Throws_OnNonSuccessStatusCode()
        {
            // Arrange
            var model = new RecipeEditModel { Id = 1 };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{RecipePath}")
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
            var model = new RecipeEditModel { Id = 2 };
            var expectedResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{RecipePath}")
                .With(m =>
                {
                    var body = m.Content!.ReadAsStringAsync().Result;
                    var deserialized = JsonSerializer.Deserialize<RecipeEditModel>(body, JsonOptions);
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
            var model = new RecipeEditModel { Id = 2 };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{RecipePath}")
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
                .Expect(HttpMethod.Delete, $"{BaseAddress}{RecipePath}*")
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
                .Expect(HttpMethod.Delete, $"{BaseAddress}{RecipePath}*")
                .With(m => m.RequestUri!.Query.Contains($"id={id}"))
                .Respond(HttpStatusCode.NotFound);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.DeleteAsync(id));
            mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}