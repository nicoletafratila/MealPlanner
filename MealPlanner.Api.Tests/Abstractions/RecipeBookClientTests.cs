using System.Text.Json;
using Common.Api;
using Common.Constants;
using MealPlanner.Api.Abstractions;
using Microsoft.Extensions.Configuration;
using RecipeBook.Shared.Models;
using RichardSzalay.MockHttp;

namespace MealPlanner.Api.Tests.Abstractions
{
    [TestFixture]
    public class RecipeBookClientTests
    {
        private const string BaseAddress = "https://api.test/";
        private const string RecipeCategoryPath = "api/recipecategory";
        private const string ProductCategoryPath = "api/productcategory";

        private static JsonSerializerOptions JsonOptions => new(JsonSerializerDefaults.Web);

        private static RecipeBookApiConfig CreateConfig()
        {
            var configuration = new ConfigurationBuilder().Build();
            var cfg = new RecipeBookApiConfig(configuration)
            {
                BaseUrl = new Uri(BaseAddress),
                Controllers = new Dictionary<string, string>
                {
                    [RecipeBookControllers.RecipeCategory] = RecipeCategoryPath,
                    [RecipeBookControllers.ProductCategory] = ProductCategoryPath
                }
            };

            return cfg;
        }

        private static (RecipeBookClient client, MockHttpMessageHandler mockHttp) CreateClient()
        {
            var mockHttp = new MockHttpMessageHandler();

            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(BaseAddress)
            };

            var config = CreateConfig();
            var client = new RecipeBookClient(httpClient, config);

            return (client, mockHttp);
        }

        // -------- GetCategoriesAsync --------
        [Test]
        public async Task GetCategoriesAsync_CallsCorrectUrlAndReturnsData()
        {
            // Arrange
            var (client, mockHttp) = CreateClient();

            var categories = new List<RecipeCategoryModel>
            {
                new() { Id = 1, Name = "Main" },
                new() { Id = 2, Name = "Dessert" }
            };

            var url = $"{BaseAddress}{RecipeCategoryPath}/searchbycategories?categoryIds=1%2C2";

            mockHttp.When(HttpMethod.Get, url)
                .WithHeaders("Authorization", "Bearer tok")
                .Respond("application/json", JsonSerializer.Serialize(categories, JsonOptions));

            // Act
            var result = await client.GetCategoriesAsync("1,2", "tok", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result!.Select(c => c.Name), Is.EquivalentTo(new[] { "Main", "Dessert" }));

            mockHttp.VerifyNoOutstandingExpectation();
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Test]
        public async Task GetCategoriesAsync_EmptyResponse_ReturnsEmptyList()
        {
            // Arrange
            var (client, mockHttp) = CreateClient();

            var url = $"{BaseAddress}{RecipeCategoryPath}/searchbycategories?categoryIds=1%2C2";

            mockHttp.When(HttpMethod.Get, url)
                .WithHeaders("Authorization", "Bearer tok")
                .Respond("application/json", "[]");

            // Act
            var result = await client.GetCategoriesAsync("1,2", "tok", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);

            mockHttp.VerifyNoOutstandingExpectation();
            mockHttp.VerifyNoOutstandingRequest();
        }

        // -------- GetProductCategoriesAsync --------
        [Test]
        public async Task GetProductCategoriesAsync_CallsCorrectUrlAndReturnsData()
        {
            // Arrange
            var (client, mockHttp) = CreateClient();

            var categories = new List<ProductCategoryModel>
            {
                new() { Id = 10, Name = "Dairy" },
                new() { Id = 11, Name = "Bakery" }
            };

            var url = $"{BaseAddress}{ProductCategoryPath}/searchbycategories?categoryIds=10%2C11";

            mockHttp.When(HttpMethod.Get, url)
                .WithHeaders("Authorization", "Bearer tok")
                .Respond("application/json", JsonSerializer.Serialize(categories, JsonOptions));

            // Act
            var result = await client.GetProductCategoriesAsync("10,11", "tok", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result!.Select(c => c.Name), Is.EquivalentTo(new[] { "Dairy", "Bakery" }));

            mockHttp.VerifyNoOutstandingExpectation();
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Test]
        public async Task GetProductCategoriesAsync_EmptyResponse_ReturnsEmptyList()
        {
            // Arrange
            var (client, mockHttp) = CreateClient();

            var url = $"{BaseAddress}{ProductCategoryPath}/searchbycategories?categoryIds=10%2C11";

            mockHttp.When(HttpMethod.Get, url)
                .WithHeaders("Authorization", "Bearer tok")
                .Respond("application/json", "[]");

            // Act
            var result = await client.GetProductCategoriesAsync("10,11", "tok", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);

            mockHttp.VerifyNoOutstandingExpectation();
            mockHttp.VerifyNoOutstandingRequest();
        }
    }
}