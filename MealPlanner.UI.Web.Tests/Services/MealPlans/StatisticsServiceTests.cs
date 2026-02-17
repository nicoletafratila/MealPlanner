using System.Net;
using System.Text.Json;
using Blazored.SessionStorage;
using Common.Api;
using Common.Constants;
using Common.Models;
using MealPlanner.UI.Web.Services.MealPlans;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Shared.Models;
using RichardSzalay.MockHttp;

namespace MealPlanner.UI.Web.Tests.Services.MealPlans
{
    [TestFixture]
    public class StatisticsServiceTests
    {
        private const string AuthTokenKey = Common.Constants.MealPlanner.AuthToken;
        private const string BaseAddress = "https://api.test/";
        private const string StatisticsPath = "api/statistics";

        private static JsonSerializerOptions JsonOptions => new(JsonSerializerDefaults.Web);

        private static MealPlannerApiConfig CreateConfig()
        {
            var configuration = new ConfigurationBuilder().Build();
            return new MealPlannerApiConfig(configuration)
            {
                BaseUrl = new Uri(BaseAddress),
                Controllers = new Dictionary<string, string>
                {
                    [MealPlannerControllers.Statistics] = StatisticsPath
                }
            };
        }

        private static StatisticsService CreateService(
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
            var logger = Mock.Of<ILogger<StatisticsService>>();

            return new StatisticsService(httpClient, tokenProvider, config, logger);
        }

        // ---------- GetFavoriteRecipesAsync ----------
        [Test]
        public async Task GetFavoriteRecipesAsync_ReturnsList_AndSendsAuthHeaderAndQuery()
        {
            // Arrange
            const string token = "my-jwt-token";
            var categories = new List<RecipeCategoryModel>
            {
                new() { Id = 1 },
                new() { Id = 3 }
            };

            var expected = new List<StatisticModel>
            {
                new() { Title = "R1", Label = "L1" },
                new() { Title = "R2", Label = "L2" }
            };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{StatisticsPath}/favoriterecipes*")
                .With(m =>
                {
                    var auth = m.Headers.Authorization;
                    var query = m.RequestUri!.Query;
                    return auth is not null
                           && auth.Scheme == JwtBearerDefaults.AuthenticationScheme
                           && auth.Parameter == token
                           && query.Contains("categoryIds=1,3");
                })
                .Respond("application/json", JsonSerializer.Serialize(expected, JsonOptions));

            var service = CreateService(mockHttp, token);

            // Act
            var result = await service.GetFavoriteRecipesAsync(categories);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Count, Is.EqualTo(expected.Count));
            Assert.That(result[0].Title, Is.EqualTo("R1"));
            mockHttp.VerifyNoOutstandingExpectation();
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Test]
        public void GetFavoriteRecipesAsync_Throws_OnNonSuccessStatusCode()
        {
            // Arrange
            var categories = new List<RecipeCategoryModel> { new() { Id = 1 } };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{StatisticsPath}/favoriterecipes*")
                .Respond(HttpStatusCode.InternalServerError);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.GetFavoriteRecipesAsync(categories));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- GetFavoriteProductsAsync ----------
        [Test]
        public async Task GetFavoriteProductsAsync_ReturnsList_AndSendsAuthHeaderAndQuery()
        {
            // Arrange
            const string token = "my-jwt-token";
            var categories = new List<ProductCategoryModel>
            {
                new() { Id = 2 },
                new() { Id = 4 }
            };

            var expected = new List<StatisticModel>
            {
                new() { Title = "P1", Label = "L1" },
                new() { Title = "P2", Label = "L2" }
            };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{StatisticsPath}/favoriteproducts*")
                .With(m =>
                {
                    var auth = m.Headers.Authorization;
                    var query = m.RequestUri!.Query;
                    return auth is not null
                           && auth.Scheme == JwtBearerDefaults.AuthenticationScheme
                           && auth.Parameter == token
                           && query.Contains("categoryIds=2,4");
                })
                .Respond("application/json", JsonSerializer.Serialize(expected, JsonOptions));

            var service = CreateService(mockHttp, token);

            // Act
            var result = await service.GetFavoriteProductsAsync(categories);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Count, Is.EqualTo(expected.Count));
            Assert.That(result[1].Title, Is.EqualTo("P2"));
            mockHttp.VerifyNoOutstandingExpectation();
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Test]
        public void GetFavoriteProductsAsync_Throws_OnNonSuccessStatusCode()
        {
            // Arrange
            var categories = new List<ProductCategoryModel> { new() { Id = 2 } };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{StatisticsPath}/favoriteproducts*")
                .Respond(HttpStatusCode.InternalServerError);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.GetFavoriteProductsAsync(categories));
            mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}