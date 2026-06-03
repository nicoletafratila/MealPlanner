using Common.Services;
using MealPlanner.Api.Abstractions;
using MealPlanner.Api.Features.Statistics.Queries.SearchProducts;
using MealPlanner.Api.Repositories;
using Moq;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Tests.Features.Statistics.Queries.SearchProducts
{
    [TestFixture]
    public class SearchQueryHandlerTests
    {
        private static (SearchQueryHandler handler,
                        Mock<IMealPlanRepository> repoMock,
                        Mock<IRecipeBookClient> recipeClientMock)
            CreateHandler(string userId = "user1")
        {
            var repoMock = new Mock<IMealPlanRepository>(MockBehavior.Strict);
            var recipeClientMock = new Mock<IRecipeBookClient>(MockBehavior.Strict);
            var currentUserMock = new Mock<ICurrentUserService>(MockBehavior.Loose);
            currentUserMock.Setup(s => s.UserId).Returns(userId);

            var handler = new SearchQueryHandler(repoMock.Object, recipeClientMock.Object, currentUserMock.Object);
            return (handler, repoMock, recipeClientMock);
        }

        [Test]
        public async Task Handle_EmptyCategoryIds_ReturnsEmpty()
        {
            // Arrange
            var (handler, repoMock, recipeClientMock) = CreateHandler();

            var query = new SearchQuery
            {
                CategoryIds = null,
                AuthToken = "tok"
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);

            recipeClientMock.Verify(c =>
                    c.GetProductCategoriesAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);

            repoMock.Verify(r =>
                    r.SearchByProductCategoryIdsAsync(
                        It.IsAny<IList<Guid>>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_NoCategoriesReturned_ReturnsEmpty()
        {
            // Arrange
            var (handler, repoMock, recipeClientMock) = CreateHandler();

            var categoryIdsCsv = $"{Guid.NewGuid()},{Guid.NewGuid()}";

            var query = new SearchQuery
            {
                CategoryIds = categoryIdsCsv,
                AuthToken = "test-token"
            };

            recipeClientMock
                .Setup(c => c.GetProductCategoriesAsync(
                    categoryIdsCsv,
                    "test-token",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);

            repoMock.Verify(r =>
                    r.SearchByProductCategoryIdsAsync(
                        It.IsAny<IList<Guid>>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);

            recipeClientMock.Verify(c =>
                    c.GetProductCategoriesAsync(
                        categoryIdsCsv,
                        "test-token",
                        It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Handle_WithData_ComputesStatisticsPerCategory()
        {
            // Arrange
            var (handler, repoMock, recipeClientMock) = CreateHandler();

            var dairyId = Guid.NewGuid();
            var bakeryId = Guid.NewGuid();
            var categoryIdsCsv = $"{dairyId},{bakeryId}";

            var query = new SearchQuery
            {
                CategoryIds = categoryIdsCsv,
                AuthToken = "tok"
            };

            var categories = new List<ProductCategoryModel>
            {
                new() { Id = dairyId, Name = "Dairy" },
                new() { Id = bakeryId, Name = "Bakery" }
            };

            recipeClientMock
                .Setup(c => c.GetProductCategoriesAsync(
                    categoryIdsCsv,
                    "tok",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            // Two mealplans with products in Dairy and one in Bakery
            var p1 = new RecipeBook.Data.Entities.Product { Id = 1, Name = "Milk", ProductCategoryId = dairyId };
            var p2 = new RecipeBook.Data.Entities.Product { Id = 2, Name = "Cheese", ProductCategoryId = dairyId };
            var p3 = new RecipeBook.Data.Entities.Product { Id = 3, Name = "Bread", ProductCategoryId = bakeryId };

            var mp1 = new MealPlanner.Data.Entities.MealPlan { Id = Guid.NewGuid(), Name = "Plan1" };
            var mp2 = new MealPlanner.Data.Entities.MealPlan { Id = Guid.NewGuid(), Name = "Plan2" };

            var pairs = new List<KeyValuePair<RecipeBook.Data.Entities.Product, MealPlanner.Data.Entities.MealPlan>>
            {
                new(p1, mp1),
                new(p1, mp2),
                new(p2, mp1),
                new(p3, mp1)
            };

            repoMock
                .Setup(r => r.SearchByProductCategoryIdsAsync(
                    It.Is<IList<Guid>>(ids =>
                        ids.Count == 2 && ids.Contains(dairyId) && ids.Contains(bakeryId)),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
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
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(dairyStat.Data!.ContainsKey("Milk"), Is.True);
                    Assert.That(dairyStat.Data["Milk"], Is.EqualTo(2));
                    Assert.That(dairyStat.Data.ContainsKey("Cheese"), Is.False);
                    Assert.That(dairyStat.Data.ContainsKey("Others"), Is.True);
                    Assert.That(dairyStat.Data["Others"], Is.EqualTo(1));
                }
            }

            // Bakery: Bread(1) => goes into Others
            using (Assert.EnterMultipleScope())
            {
                Assert.That(bakeryStat.Data, Is.Not.Null);
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(bakeryStat.Data!.ContainsKey("Bread"), Is.False);
                    Assert.That(bakeryStat.Data.ContainsKey("Others"), Is.True);
                    Assert.That(bakeryStat.Data["Others"], Is.EqualTo(1));
                }
            }

            recipeClientMock.Verify(c =>
                    c.GetProductCategoriesAsync(
                        categoryIdsCsv,
                        "tok",
                        It.IsAny<CancellationToken>()),
                Times.Once);

            repoMock.Verify(r =>
                    r.SearchByProductCategoryIdsAsync(
                        It.IsAny<IList<Guid>>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}