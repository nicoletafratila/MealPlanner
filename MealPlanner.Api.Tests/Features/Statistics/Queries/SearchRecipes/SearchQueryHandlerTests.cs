using System.Text.Json;
using Common.Api;
using Common.Constants;
using MealPlanner.Api.Features.Statistics.Queries.SearchRecipes;
using MealPlanner.Api.Repositories;
using Microsoft.Extensions.Configuration;
using Moq;
using RecipeBook.Shared.Models;
using RichardSzalay.MockHttp;

namespace MealPlanner.Api.Tests.Features.Statistics.Queries.SearchRecipes
{
    [TestFixture]
    public class SearchQueryHandlerTests
    {
        private const string BaseAddress = "https://api.test/";
        private const string RecipeCategoryPath = "api/recipecategory";

        private static JsonSerializerOptions JsonOptions => new(JsonSerializerDefaults.Web);

        private static RecipeBookApiConfig CreateConfig()
        {
            var configuration = new ConfigurationBuilder().Build();
            var cfg = new RecipeBookApiConfig(configuration)
            {
                BaseUrl = new Uri(BaseAddress),
                Controllers = new Dictionary<string, string>
                {
                    [RecipeBookControllers.RecipeCategory] = RecipeCategoryPath
                }
            };

            return cfg;
        }

        private static (SearchQueryHandler handler, Mock<IMealPlanRepository> mealPlanRepoMock, MockHttpMessageHandler mockHttp)
            CreateHandler()
        {
            var mockHttp = new MockHttpMessageHandler();

            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(BaseAddress)
            };

            var mealPlanRepoMock = new Mock<IMealPlanRepository>(MockBehavior.Strict);
            var config = CreateConfig();

            var handler = new SearchQueryHandler(mealPlanRepoMock.Object, config, httpClient);
            return (handler, mealPlanRepoMock, mockHttp);
        }

        [Test]
        public async Task Handle_EmptyCategoryIds_ReturnsEmpty()
        {
            // Arrange
            var (handler, mealPlanRepoMock, mockHttp) = CreateHandler();

            var query = new SearchQuery
            {
                CategoryIds = null,
                AuthToken = "test-token"
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);
            mealPlanRepoMock.Verify(
                r => r.SearchByRecipeCategoryIdsAsync(It.IsAny<IList<int>>()),
                Times.Never);

            // No HTTP calls expected
            mockHttp.VerifyNoOutstandingExpectation();
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Test]
        public async Task Handle_NoCategoriesReturned_ReturnsEmpty()
        {
            // Arrange
            var (handler, mealPlanRepoMock, mockHttp) = CreateHandler();

            var query = new SearchQuery
            {
                CategoryIds = "1,2",
                AuthToken = "test-token"
            };

            // Mock RecipeCategory API returning empty list
            var url = $"{BaseAddress}{RecipeCategoryPath}/searchbycategories?categoryIds=1%2C2";
            mockHttp.When(HttpMethod.Get, url)
                .WithHeaders("Authorization", "Bearer test-token")
                .Respond("application/json", "[]");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);

            mealPlanRepoMock.Verify(
                r => r.SearchByRecipeCategoryIdsAsync(It.IsAny<IList<int>>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_WithData_ComputesStatisticsPerCategory()
        {
            // Arrange
            var (handler, mealPlanRepoMock, mockHttp) = CreateHandler();

            var query = new SearchQuery
            {
                CategoryIds = "5,6",
                AuthToken = "test-token"
            };

            var categories = new List<RecipeCategoryModel>
            {
                new() { Id = 5, Name = "Main" },
                new() { Id = 6, Name = "Dessert" }
            };

            // Mock RecipeCategory API
            var url = $"{BaseAddress}{RecipeCategoryPath}/searchbycategories?categoryIds=5%2C6";
            mockHttp.When(HttpMethod.Get, url)
                .WithHeaders("Authorization", "Bearer test-token")
                .Respond("application/json", JsonSerializer.Serialize(categories, JsonOptions));

            // MealPlanRecipes: 3 in Main (R1 * 2, R2 * 1) and 1 in Dessert (D1 * 1)
            var mealPlanRecipes = new List<Common.Data.Entities.MealPlanRecipe>
            {
                new()
                {
                    Recipe = new Common.Data.Entities.Recipe
                    {
                        Id = 1,
                        Name = "R1",
                        RecipeCategoryId = 5
                    }
                },
                new()
                {
                    Recipe = new Common.Data.Entities.Recipe
                    {
                        Id = 2,
                        Name = "R1",
                        RecipeCategoryId = 5
                    }
                },
                new()
                {
                    Recipe = new Common.Data.Entities.Recipe
                    {
                        Id = 3,
                        Name = "R2",
                        RecipeCategoryId = 5
                    }
                },
                new()
                {
                    Recipe = new Common.Data.Entities.Recipe
                    {
                        Id = 4,
                        Name = "D1",
                        RecipeCategoryId = 6
                    }
                }
            };

            mealPlanRepoMock
                .Setup(r => r.SearchByRecipeCategoryIdsAsync(It.Is<IList<int>>(ids =>
                    ids.Count == 2 && ids.Contains(5) && ids.Contains(6))))
                .ReturnsAsync(mealPlanRecipes);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));

            var mainStat = result.Single(s => s.Title == "Main");
            var dessertStat = result.Single(s => s.Title == "Dessert");

            // Main: R1(2), R2(1) => R1 kept, R2 under "Others"
            using (Assert.EnterMultipleScope())
            {
                Assert.That(mainStat.Data, Is.Not.Null);
                Assert.Multiple(() =>
                {
                    Assert.That(mainStat.Data!.ContainsKey("R1"), Is.True);
                    Assert.That(mainStat.Data["R1"], Is.EqualTo(2));
                    Assert.That(mainStat.Data.ContainsKey("Others"), Is.True);
                    Assert.That(mainStat.Data["Others"], Is.EqualTo(1));
                });
            }

            // Dessert: D1(1) => goes to "Others"
            using (Assert.EnterMultipleScope())
            {
                Assert.That(dessertStat.Data, Is.Not.Null);
                Assert.Multiple(() =>
                {
                    Assert.That(dessertStat.Data!.ContainsKey("D1"), Is.False);
                    Assert.That(dessertStat.Data.ContainsKey("Others"), Is.True);
                    Assert.That(dessertStat.Data["Others"], Is.EqualTo(1));
                });
            }

            mealPlanRepoMock.Verify(
                r => r.SearchByRecipeCategoryIdsAsync(It.IsAny<IList<int>>()),
                Times.Once);
        }
    }
}