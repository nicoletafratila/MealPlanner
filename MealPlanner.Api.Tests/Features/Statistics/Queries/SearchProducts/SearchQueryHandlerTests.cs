using System.Text.Json;
using Common.Api;
using Common.Constants;
using MealPlanner.Api.Features.Statistics.Queries.SearchProducts;
using MealPlanner.Api.Repositories;
using Microsoft.Extensions.Configuration;
using Moq;
using RecipeBook.Shared.Models;
using RichardSzalay.MockHttp;

namespace MealPlanner.Api.Tests.Features.Statistics.Queries.SearchProducts
{
    [TestFixture]
    public class SearchQueryHandlerTests
    {
        private const string BaseAddress = "https://api.test/";
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
                    [RecipeBookControllers.ProductCategory] = ProductCategoryPath
                }
            };

            return cfg;
        }

        private static (SearchQueryHandler handler, Mock<IMealPlanRepository> repoMock, MockHttpMessageHandler mockHttp) CreateHandler()
        {
            var mockHttp = new MockHttpMessageHandler();

            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(BaseAddress)
            };

            var repoMock = new Mock<IMealPlanRepository>(MockBehavior.Strict);
            var config = CreateConfig();

            var handler = new SearchQueryHandler(repoMock.Object, config, httpClient);
            return (handler, repoMock, mockHttp);
        }

        [Test]
        public async Task Handle_EmptyCategoryIds_ReturnsEmpty()
        {
            // Arrange
            var (handler, repoMock, mockHttp) = CreateHandler();

            var query = new SearchQuery
            {
                CategoryIds = null,
                AuthToken = "tok"
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);
            repoMock.Verify(r => r.SearchByProductCategoryIdsAsync(It.IsAny<IList<int>>()), Times.Never);

            // No HTTP calls expected
            mockHttp.VerifyNoOutstandingExpectation();
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Test]
        public async Task Handle_NoCategoriesReturned_ReturnsEmpty()
        {
            // Arrange
            var (handler, repoMock, mockHttp) = CreateHandler();

            var query = new SearchQuery
            {
                CategoryIds = "1,2",
                AuthToken = "test-token"
            };

            var url = $"{BaseAddress}{ProductCategoryPath}/searchbycategories?categoryIds=1%2C2";
            mockHttp.When(HttpMethod.Get, url)
                .WithHeaders("Authorization", "Bearer test-token")
                .Respond("application/json", "[]");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);
            repoMock.Verify(r => r.SearchByProductCategoryIdsAsync(It.IsAny<IList<int>>()), Times.Never);
        }

        [Test]
        public async Task Handle_WithData_ComputesStatisticsPerCategory()
        {
            // Arrange
            var (handler, repoMock, mockHttp) = CreateHandler();

            var query = new SearchQuery
            {
                CategoryIds = "20,21",
                AuthToken = "tok"
            };

            var categories = new List<ProductCategoryModel>
            {
                new() { Id = 20, Name = "Dairy" },
                new() { Id = 21, Name = "Bakery" }
            };

            var url = $"{BaseAddress}{ProductCategoryPath}/searchbycategories?categoryIds=20%2C21";
            mockHttp.When(HttpMethod.Get, url)
                .WithHeaders("Authorization", "Bearer tok")
                .Respond("application/json", JsonSerializer.Serialize(categories, JsonOptions));

            // Two mealplans with products in Dairy and one in Bakery
            var p1 = new Common.Data.Entities.Product { Id = 1, Name = "Milk", ProductCategoryId = 20 };
            var p2 = new Common.Data.Entities.Product { Id = 2, Name = "Cheese", ProductCategoryId = 20 };
            var p3 = new Common.Data.Entities.Product { Id = 3, Name = "Bread", ProductCategoryId = 21 };

            var mp1 = new Common.Data.Entities.MealPlan { Id = 100, Name = "Plan1" };
            var mp2 = new Common.Data.Entities.MealPlan { Id = 101, Name = "Plan2" };

            var pairs = new List<KeyValuePair<Common.Data.Entities.Product, Common.Data.Entities.MealPlan>>
            {
                new(p1, mp1),
                new(p1, mp2),
                new(p2, mp1),
                new(p3, mp1)
            };

            repoMock
                .Setup(r => r.SearchByProductCategoryIdsAsync(It.Is<IList<int>>(ids =>
                    ids.Count == 2 && ids.Contains(20) && ids.Contains(21))))
                .ReturnsAsync(pairs);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));

            var dairyStat = result.Single(s => s.Title == "Dairy");
            var bakeryStat = result.Single(s => s.Title == "Bakery");

            // Dairy: Milk(2), Cheese(1) => Milk kept, Cheese aggregated into Others(1)
            using (Assert.EnterMultipleScope())
            {
                Assert.That(dairyStat.Data, Is.Not.Null);
                Assert.Multiple(() =>
                {
                    Assert.That(dairyStat.Data!.ContainsKey("Milk"), Is.True);
                    Assert.That(dairyStat.Data["Milk"], Is.EqualTo(2));
                    Assert.That(dairyStat.Data.ContainsKey("Cheese"), Is.False);
                    Assert.That(dairyStat.Data.ContainsKey("Others"), Is.True);
                    Assert.That(dairyStat.Data["Others"], Is.EqualTo(1));
                });
            }

            // Bakery: Bread(1) => goes into Others
            using (Assert.EnterMultipleScope())
            {
                Assert.That(bakeryStat.Data, Is.Not.Null);
                Assert.Multiple(() =>
                {
                    Assert.That(bakeryStat.Data!.ContainsKey("Bread"), Is.False);
                    Assert.That(bakeryStat.Data.ContainsKey("Others"), Is.True);
                    Assert.That(bakeryStat.Data["Others"], Is.EqualTo(1));
                });
            }

            repoMock.Verify(r => r.SearchByProductCategoryIdsAsync(It.IsAny<IList<int>>()), Times.Once);
        }
    }
}