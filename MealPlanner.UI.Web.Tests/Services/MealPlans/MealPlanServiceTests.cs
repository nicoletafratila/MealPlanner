using System.Net;
using System.Text.Json;
using Blazored.SessionStorage;
using Common.Api;
using Common.Constants;
using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services.MealPlans;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;

namespace MealPlanner.UI.Web.Tests.Services.MealPlans
{
    [TestFixture]
    public class MealPlanServiceTests
    {
        private const string AuthTokenKey = Common.Constants.MealPlanner.AuthToken;
        private const string BaseAddress = "https://api.test/";
        private const string MealPlanPath = "api/mealplan";

        private static JsonSerializerOptions JsonOptions => new(JsonSerializerDefaults.Web);

        private static MealPlannerApiConfig CreateConfig()
        {
            var configuration = new ConfigurationBuilder().Build();
            var cfg = new MealPlannerApiConfig(configuration)
            {
                BaseUrl = new Uri(BaseAddress),
                Controllers = new Dictionary<string, string>
                {
                    [MealPlannerControllers.MealPlan] = MealPlanPath
                }
            };

            return cfg;
        }

        private static MealPlanService CreateService(
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
            var logger = Mock.Of<ILogger<MealPlanService>>();

            return new MealPlanService(httpClient, tokenProvider, config, logger);
        }

        // ---------- GetEditAsync ----------
        [Test]
        public async Task GetEditAsync_ReturnsDeserializedModel_AndSendsAuthHeader()
        {
            // Arrange
            const string token = "my-jwt-token";
            var mealPlanId = 42;
            var expected = new MealPlanEditModel { Id = mealPlanId };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{MealPlanPath}/edit*")
                .With(m =>
                {
                    var auth = m.Headers.Authorization;
                    return auth is not null
                           && auth.Scheme == JwtBearerDefaults.AuthenticationScheme
                           && auth.Parameter == token
                           && m.RequestUri!.Query.Contains($"id={mealPlanId}");
                })
                .Respond("application/json", JsonSerializer.Serialize(expected, JsonOptions));

            var service = CreateService(mockHttp, token);

            // Act
            var result = await service.GetEditAsync(mealPlanId);

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
            var mealPlanId = 10;
            var shopId = 5;

            var expected = new List<ShoppingListProductEditModel>
            {
                new ShoppingListProductEditModel(),
                new ShoppingListProductEditModel()
            };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{MealPlanPath}/shoppingListProducts*")
                .With(m =>
                {
                    var q = m.RequestUri!.Query;
                    return q.Contains($"mealPlanId={mealPlanId}") && q.Contains($"shopId={shopId}");
                })
                .Respond("application/json", JsonSerializer.Serialize(expected, JsonOptions));

            var service = CreateService(mockHttp);

            // Act
            var result = await service.GetShoppingListProductsAsync(mealPlanId, shopId);

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

            var paged = new PagedList<MealPlanModel>(
                new[]
                {
                    new MealPlanModel(),
                    new MealPlanModel()
                },
                metadata);

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{MealPlanPath}/search*")
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
                .Expect(HttpMethod.Get, $"{BaseAddress}{MealPlanPath}/search*")
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
            var model = new MealPlanEditModel { Id = 1 };
            var expectedResponse = new CommandResponse { Succeeded = true, Message = "ok" };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{MealPlanPath}")
                .With(m =>
                {
                    var body = m.Content!.ReadAsStringAsync().Result;
                    var deserialized = JsonSerializer.Deserialize<MealPlanEditModel>(body, JsonOptions);
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
            var model = new MealPlanEditModel { Id = 1 };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{MealPlanPath}")
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
            var model = new MealPlanEditModel { Id = 2 };
            var expectedResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{MealPlanPath}")
                .With(m =>
                {
                    var body = m.Content!.ReadAsStringAsync().Result;
                    var deserialized = JsonSerializer.Deserialize<MealPlanEditModel>(body, JsonOptions);
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
            var model = new MealPlanEditModel { Id = 2 };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{MealPlanPath}")
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
                .Expect(HttpMethod.Delete, $"{BaseAddress}{MealPlanPath}*")
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
                .Expect(HttpMethod.Delete, $"{BaseAddress}{MealPlanPath}*")
                .With(m => m.RequestUri!.Query.Contains($"id={id}"))
                .Respond(HttpStatusCode.NotFound);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.DeleteAsync(id));
            mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}