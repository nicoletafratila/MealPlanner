using Common.Services;
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
            CreateHandler(string userId = "user1")
        {
            var mealPlanRepoMock = new Mock<IMealPlanRepository>(MockBehavior.Strict);
            var recipeBookClientMock = new Mock<IRecipeBookClient>(MockBehavior.Strict);
            var currentUserMock = new Mock<ICurrentUserService>(MockBehavior.Loose);
            currentUserMock.Setup(s => s.UserId).Returns(userId);

            var handler = new SearchQueryHandler(mealPlanRepoMock.Object, recipeBookClientMock.Object, currentUserMock.Object);
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
                r => r.SearchByRecipeCategoryIdsAsync(It.IsAny<IList<Guid>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_NoCategoriesReturned_ReturnsEmpty()
        {
            // Arrange
            var (handler, mealPlanRepoMock, recipeBookClientMock) = CreateHandler();

            var categoryIds = $"{Guid.NewGuid()},{Guid.NewGuid()}";

            var query = new SearchQuery
            {
                CategoryIds = categoryIds,
                AuthToken = "test-token"
            };

            recipeBookClientMock
                .Setup(c => c.GetCategoriesAsync(
                    categoryIds,
                    "test-token",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);

            mealPlanRepoMock.Verify(
                r => r.SearchByRecipeCategoryIdsAsync(It.IsAny<IList<Guid>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);

            recipeBookClientMock.Verify(
                c => c.GetCategoriesAsync(
                    categoryIds,
                    "test-token",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Handle_WithData_ComputesStatisticsPerCategory()
        {
            // Arrange
            var (handler, mealPlanRepoMock, recipeBookClientMock) = CreateHandler();

            var cat1Id = Guid.NewGuid();
            var cat2Id = Guid.NewGuid();
            var categoryIds = $"{cat1Id},{cat2Id}";

            var query = new SearchQuery
            {
                CategoryIds = categoryIds,
                AuthToken = "test-token"
            };

            var categories = new List<RecipeCategoryModel>
            {
                new() { Id = cat1Id, Name = "Main" },
                new() { Id = cat2Id, Name = "Dessert" }
            };

            recipeBookClientMock
                .Setup(c => c.GetCategoriesAsync(
                    categoryIds,
                    "test-token",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            // MealPlanRecipes: 3 in Main (R1 * 2, R2 * 1) and 1 in Dessert (D1 * 1)
            var mealPlanRecipes = new List<MealPlanner.Data.Entities.MealPlanRecipe>
            {
                new()
                {
                    Recipe = new RecipeBook.Data.Entities.Recipe
                    {
                        Id = Guid.NewGuid(),
                        Name = "R1",
                        RecipeCategoryId = cat1Id
                    }
                },
                new()
                {
                    Recipe = new RecipeBook.Data.Entities.Recipe
                    {
                        Id = Guid.NewGuid(),
                        Name = "R1",
                        RecipeCategoryId = cat1Id
                    }
                },
                new()
                {
                    Recipe = new RecipeBook.Data.Entities.Recipe
                    {
                        Id = Guid.NewGuid(),
                        Name = "R2",
                        RecipeCategoryId = cat1Id
                    }
                },
                new()
                {
                    Recipe = new RecipeBook.Data.Entities.Recipe
                    {
                        Id = Guid.NewGuid(),
                        Name = "D1",
                        RecipeCategoryId = cat2Id
                    }
                }
            };

            mealPlanRepoMock
                .Setup(r => r.SearchByRecipeCategoryIdsAsync(
                    It.Is<IList<Guid>>(ids => ids.Count == 2 && ids.Contains(cat1Id) && ids.Contains(cat2Id)),
                    It.IsAny<string>(),
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
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(mainStat.Data!.ContainsKey("R1"), Is.True);
                    Assert.That(mainStat.Data["R1"], Is.EqualTo(2));
                    Assert.That(mainStat.Data.ContainsKey("Others"), Is.True);
                    Assert.That(mainStat.Data["Others"], Is.EqualTo(1));
                }
            }

            // Dessert: D1(1) => goes to "Others"
            using (Assert.EnterMultipleScope())
            {
                Assert.That(dessertStat.Data, Is.Not.Null);
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(dessertStat.Data!.ContainsKey("D1"), Is.False);
                    Assert.That(dessertStat.Data.ContainsKey("Others"), Is.True);
                    Assert.That(dessertStat.Data["Others"], Is.EqualTo(1));
                }
            }

            recipeBookClientMock.Verify(
                c => c.GetCategoriesAsync(
                    categoryIds,
                    "test-token",
                    It.IsAny<CancellationToken>()),
                Times.Once);

            mealPlanRepoMock.Verify(
                r => r.SearchByRecipeCategoryIdsAsync(It.IsAny<IList<Guid>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}