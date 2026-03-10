using System.Text.Json;
using Common.Api;
using Common.Constants;
using MealPlanner.Shared.Models;
using RecipeBook.Api.Abstractions;
using RichardSzalay.MockHttp;

namespace RecipeBook.Api.Tests.Abstractions
{
    [TestFixture]
    public class MealPlannerClientTests
    {
        private const string BaseAddress = "https://mealplanner.test/";
        private const string ShopPath = "api/shop";
        private const string MealPlanPath = "api/mealplan";

        private static JsonSerializerOptions JsonOptions => new(JsonSerializerDefaults.Web);

        private static IApiConfig CreateConfig()
        {
            var configMock = new Moq.Mock<IApiConfig>(Moq.MockBehavior.Strict);

            configMock.SetupGet(c => c.BaseUrl)
                .Returns(new Uri(BaseAddress));

            configMock.SetupGet(c => c.Controllers)
                .Returns(new Dictionary<string, string>
                {
                    { MealPlannerControllers.Shop, ShopPath },
                    { MealPlannerControllers.MealPlan, MealPlanPath }
                });

            return configMock.Object;
        }

        private static (MealPlannerClient client, MockHttpMessageHandler mockHttp, IApiConfig config) CreateClient()
        {
            var mockHttp = new MockHttpMessageHandler();

            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(BaseAddress)
            };

            var config = CreateConfig();
            var client = new MealPlannerClient(httpClient, config);

            return (client, mockHttp, config);
        }

        [Test]
        public void Ctor_NullHttpClient_Throws()
        {
            var config = CreateConfig();

            Assert.Throws<ArgumentNullException>(() =>
                _ = new MealPlannerClient(null!, config));
        }

        [Test]
        public void Ctor_NullApiConfig_Throws()
        {
            var mockHttp = new MockHttpMessageHandler();
            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(BaseAddress)
            };

            Assert.Throws<ArgumentNullException>(() =>
                _ = new MealPlannerClient(httpClient, null!));
        }

        [Test]
        public void GetShopAsync_InvalidShopId_Throws()
        {
            var (client, _, _) = CreateClient();

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
                await client.GetShopAsync(0, "token", CancellationToken.None));
        }

        [Test]
        public async Task GetShopAsync_CallsExpectedUrl_WithAuthHeader()
        {
            // Arrange
            var (client, mockHttp, _) = CreateClient();

            var expectedShop = new ShopEditModel
            {
                Id = 5,
                Name = "TestShop"
            };

            var url = $"{BaseAddress}{ShopPath}/edit?id=5";

            mockHttp.When(HttpMethod.Get, url)
                .WithHeaders("Authorization", "Bearer abc")
                .Respond("application/json", JsonSerializer.Serialize(expectedShop, JsonOptions));

            // Act
            var result = await client.GetShopAsync(5, "abc", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Id, Is.EqualTo(5));
                Assert.That(result.Name, Is.EqualTo("TestShop"));
            }

            mockHttp.VerifyNoOutstandingExpectation();
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Test]
        public void GetMealPlansByRecipeIdAsync_InvalidRecipeId_Throws()
        {
            var (client, _, _) = CreateClient();

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
                await client.GetMealPlansByRecipeIdAsync(0, "token", CancellationToken.None));
        }

        [Test]
        public async Task GetMealPlansByRecipeIdAsync_CallsExpectedUrl_AndDeserializesList()
        {
            // Arrange
            var (client, mockHttp, _) = CreateClient();

            var plans = new List<MealPlanModel>
            {
                new() { Id = 1, Name = "Plan1" }
            };

            var url = $"{BaseAddress}{MealPlanPath}/searchbyid?id=7";

            mockHttp.When(HttpMethod.Get, url)
                .WithHeaders("Authorization", "Bearer tok")
                .Respond("application/json", JsonSerializer.Serialize(plans, JsonOptions));

            // Act
            var result = await client.GetMealPlansByRecipeIdAsync(7, "tok", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!, Has.Count.EqualTo(1));
                Assert.That(result[0].Name, Is.EqualTo("Plan1"));
            }

            mockHttp.VerifyNoOutstandingExpectation();
            mockHttp.VerifyNoOutstandingRequest();
        }
    }
}