using System.Net;
using System.Net.Http.Json;
using Common.Api;
using Common.Constants;
using MealPlanner.Shared.Models;
using Moq;
using Moq.Protected;
using RecipeBook.Api.Abstractions;

namespace RecipeBook.Api.Tests.Abstractions
{
    [TestFixture]
    public class MealPlannerClientTests
    {
        private Mock<IApiConfig> _apiConfigMock = null!;
        private HttpClient _httpClient = null!;
        private Mock<HttpMessageHandler> _handlerMock = null!;
        private MealPlannerClient _client = null!;

        [SetUp]
        public void SetUp()
        {
            _apiConfigMock = new Mock<IApiConfig>(MockBehavior.Strict);

            _apiConfigMock.SetupGet(c => c.BaseUrl)
                .Returns(new Uri("https://mealplanner.test/"));

            _apiConfigMock.SetupGet(c => c.Controllers)
                .Returns(new Dictionary<string, string>
                {
                    { MealPlannerControllers.Shop, "api/shop" },
                    { MealPlannerControllers.MealPlan, "api/mealplan" }
                });

            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose); 

            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("https://mealplanner.test/")
            };

            _client = new MealPlannerClient(_httpClient, _apiConfigMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }

        [Test]
        public void Ctor_NullHttpClient_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new MealPlannerClient(null!, _apiConfigMock.Object));
        }

        [Test]
        public void Ctor_NullApiConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new MealPlannerClient(_httpClient, null!));
        }

        [Test]
        public void GetShopAsync_InvalidShopId_Throws()
        {
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
                await _client.GetShopAsync(0, "token", CancellationToken.None));
        }

        [Test]
        public async Task GetShopAsync_CallsExpectedUrl_WithAuthHeader()
        {
            // Arrange
            var expectedShop = new ShopEditModel
            {
                Id = 5,
                Name = "TestShop"
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedShop)
            };

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == "https://mealplanner.test/api/shop/edit?id=5" &&
                        req.Headers.Authorization != null &&
                        req.Headers.Authorization.Scheme == "Bearer" &&
                        req.Headers.Authorization.Parameter == "abc"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _client.GetShopAsync(5, "abc", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Id, Is.EqualTo(5));
                Assert.That(result.Name, Is.EqualTo("TestShop"));
            }

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public void GetMealPlansByRecipeIdAsync_InvalidRecipeId_Throws()
        {
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
                await _client.GetMealPlansByRecipeIdAsync(0, "token", CancellationToken.None));
        }

        [Test]
        public async Task GetMealPlansByRecipeIdAsync_CallsExpectedUrl_AndDeserializesList()
        {
            // Arrange
            var plans = new List<MealPlanModel>
            {
                new() { Id = 1, Name = "Plan1" }
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(plans)
            };

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == "https://mealplanner.test/api/mealplan/searchbyid?id=7" &&
                        req.Headers.Authorization != null &&
                        req.Headers.Authorization.Scheme == "Bearer" &&
                        req.Headers.Authorization.Parameter == "tok"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _client.GetMealPlansByRecipeIdAsync(7, "tok", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!, Has.Count.EqualTo(1));
                Assert.That(result[0].Name, Is.EqualTo("Plan1"));
            }

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}