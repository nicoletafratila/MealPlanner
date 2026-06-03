using System.Net;
using System.Text.Json;
using Common.Http;
using Common.Models;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Shared.Models;
using RichardSzalay.MockHttp;

namespace MealPlanner.Services.Http.Tests
{
    [TestFixture]
    public class StatisticsServiceTests
    {
        private const string BaseAddress = "https://api.test/";
        private const string StatisticsPath = "api/statistics";

        private static JsonSerializerOptions JsonOptions => new(JsonSerializerDefaults.Web);

        private static StatisticsService CreateService(
            MockHttpMessageHandler mockHttp,
            string token = "test-token")
        {
            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(BaseAddress)
            };

            var tokenProvider = new Mock<ITokenProvider>();
            tokenProvider
                .Setup(t => t.GetTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);
            var logger = Mock.Of<ILogger<StatisticsService>>();

            return new StatisticsService(httpClient, tokenProvider.Object, logger);
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
                    return Uri.UnescapeDataString(query).Contains("categoryIds=1,3");
                })
                .Respond("application/json", JsonSerializer.Serialize(expected, JsonOptions));

            var service = CreateService(mockHttp, token);

            // Act
            var result = await service.GetFavoriteRecipesAsync(categories);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!, Has.Count.EqualTo(expected.Count));
                Assert.That(result[0].Title, Is.EqualTo("R1"));
            }
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
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var categories = new List<ProductCategoryModel>
            {
                new() { Id = id1 },
                new() { Id = id2 }
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
                    return Uri.UnescapeDataString(query).Contains($"categoryIds={id1},{id2}");
                })
                .Respond("application/json", JsonSerializer.Serialize(expected, JsonOptions));

            var service = CreateService(mockHttp, token);

            // Act
            var result = await service.GetFavoriteProductsAsync(categories);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!, Has.Count.EqualTo(expected.Count));
                Assert.That(result[1].Title, Is.EqualTo("P2"));
            }
            mockHttp.VerifyNoOutstandingExpectation();
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Test]
        public void GetFavoriteProductsAsync_Throws_OnNonSuccessStatusCode()
        {
            // Arrange
            var categories = new List<ProductCategoryModel> { new() { Id = Guid.NewGuid() } };

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
