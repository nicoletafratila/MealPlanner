using System.Text.Json;using MealPlanner.Shared.Constants; using MealPlanner.Shared.Models; using Microsoft.Extensions.Configuration; using RecipeBook.Api.Abstractions; using RichardSzalay.MockHttp;

namespace RecipeBook.Api.Tests.Abstractions
{
    [TestFixture]
    public class MealPlannerClientTests
    {
        private const string BaseAddress = "https://mealplanner.test/";
        private const string ShopPath = "api/shop";
        private const string MealPlanPath = "api/mealplan";

        private static JsonSerializerOptions JsonOptions => new(JsonSerializerDefaults.Web);

        private static MealPlannerClientConfig CreateConfig()
        {
            var configValues = new Dictionary<string, string?>
            {
                [$"{ApiConfigNames.MealPlanner}:BaseUrl"] = BaseAddress
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();
            return new MealPlannerClientConfig(configuration);
        }

        private static (MealPlannerClient client, MockHttpMessageHandler mockHttp, MealPlannerClientConfig config) CreateClient()
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
                _ = new MealPlannerClient(httpClient, (MealPlannerClientConfig)null!));
        }

        [Test]
        public void GetShopAsync_InvalidShopId_Throws()
        {
            var (client, _, _) = CreateClient();

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
                await client.GetShopAsync(Guid.Empty, "token", CancellationToken.None));
        }

        [Test]
        public async Task GetShopAsync_CallsExpectedUrl_WithAuthHeader()
        {
            // Arrange
            var (client, mockHttp, _) = CreateClient();

            var shopId = Guid.NewGuid();
            var expectedShop = new ShopEditModel
            {
                Id = shopId,
                Name = "TestShop"
            };

            var url = $"{BaseAddress}{ShopPath}/edit?id={shopId}";

            mockHttp.When(HttpMethod.Get, url)
                .WithHeaders("Authorization", "Bearer abc")
                .Respond("application/json", JsonSerializer.Serialize(expectedShop, JsonOptions));

            // Act
            var result = await client.GetShopAsync(shopId, "abc", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Id, Is.EqualTo(shopId));
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
                await client.GetMealPlansByRecipeIdAsync(Guid.Empty, "token", CancellationToken.None));
        }

        [Test]
        public async Task GetMealPlansByRecipeIdAsync_CallsExpectedUrl_AndDeserializesList()
        {
            // Arrange
            var (client, mockHttp, _) = CreateClient();

            var plans = new List<MealPlanModel>
            {
                new() { Id = Guid.NewGuid(), Name = "Plan1" }
            };

            var recipeId = Guid.NewGuid();
            var url = $"{BaseAddress}{MealPlanPath}/searchbyid?id={recipeId}";

            mockHttp.When(HttpMethod.Get, url)
                .WithHeaders("Authorization", "Bearer tok")
                .Respond("application/json", JsonSerializer.Serialize(plans, JsonOptions));

            // Act
            var result = await client.GetMealPlansByRecipeIdAsync(recipeId, "tok", CancellationToken.None);

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