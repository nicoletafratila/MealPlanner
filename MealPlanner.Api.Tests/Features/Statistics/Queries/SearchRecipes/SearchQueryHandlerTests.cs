using MealPlanner.Api.Abstractions;
using MealPlanner.Api.Features.Statistics.Queries.SearchRecipes;
using MealPlanner.Api.Repositories;
using Moq;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Tests.Features.Statistics.Queries.SearchRecipes
{
    [TestFixture]
    public class SearchQueryHandlerTests
    {
        private static (SearchQueryHandler handler,
                        Mock<IMealPlanRepository> mealPlanRepoMock,
                        Mock<IRecipeBookClient> recipeBookClientMock)
            CreateHandler()
        {
            var mealPlanRepoMock = new Mock<IMealPlanRepository>(MockBehavior.Strict);
            var recipeBookClientMock = new Mock<IRecipeBookClient>(MockBehavior.Strict);

            var handler = new SearchQueryHandler(mealPlanRepoMock.Object, recipeBookClientMock.Object);
            return (handler, mealPlanRepoMock, recipeBookClientMock);
        }

        [Test]
        public async Task Handle_EmptyCategoryIds_ReturnsEmpty()
        {
            // Arrange
            var (handler, mealPlanRepoMock, recipeBookClientMock) = CreateHandler();

            var query = new SearchQuery
            {
                CategoryIds = null,
                AuthToken = "test-token"
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);

            recipeBookClientMock.Verify(
                c => c.GetCategoriesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            mealPlanRepoMock.Verify(
                r => r.SearchByRecipeCategoryIdsAsync(It.IsAny<IList<int>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_NoCategoriesReturned_ReturnsEmpty()
        {
            // Arrange
            var (handler, mealPlanRepoMock, recipeBookClientMock) = CreateHandler();

            var query = new SearchQuery
            {
                CategoryIds = "1,2",
                AuthToken = "test-token"
            };

            recipeBookClientMock
                .Setup(c => c.GetCategoriesAsync(
                    "1,2",
                    "test-token",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);

            mealPlanRepoMock.Verify(
                r => r.SearchByRecipeCategoryIdsAsync(It.IsAny<IList<int>>(), It.IsAny<CancellationToken>()),
                Times.Never);

            recipeBookClientMock.Verify(
                c => c.GetCategoriesAsync(
                    "1,2",
                    "test-token",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Handle_WithData_ComputesStatisticsPerCategory()
        {
            // Arrange
            var (handler, mealPlanRepoMock, recipeBookClientMock) = CreateHandler();

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

            recipeBookClientMock
                .Setup(c => c.GetCategoriesAsync(
                    "5,6",
                    "test-token",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

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
                .Setup(r => r.SearchByRecipeCategoryIdsAsync(
                    It.Is<IList<int>>(ids => ids.Count == 2 && ids.Contains(5) && ids.Contains(6)),
                    It.IsAny<CancellationToken>()))
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

            recipeBookClientMock.Verify(
                c => c.GetCategoriesAsync(
                    "5,6",
                    "test-token",
                    It.IsAny<CancellationToken>()),
                Times.Once);

            mealPlanRepoMock.Verify(
                r => r.SearchByRecipeCategoryIdsAsync(It.IsAny<IList<int>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}