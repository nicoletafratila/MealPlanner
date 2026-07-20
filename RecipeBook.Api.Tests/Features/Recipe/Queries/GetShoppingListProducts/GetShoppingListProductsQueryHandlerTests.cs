using AutoMapper;
using MealPlanner.Data.Entities;
using MealPlanner.Shared.Models;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Abstractions;
using RecipeBook.Api.Features.Recipe.Queries.GetShoppingListProducts;
using RecipeBook.Api.Repositories;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.Recipe.Queries.GetShoppingListProducts
{
    [TestFixture]
    public class GetShoppingListProductsQueryHandlerTests
    {
        private Mock<IRecipeRepository> _recipeRepoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<ILogger<GetShoppingListProductsQueryHandler>> _loggerMock = null!;
        private Mock<IMealPlannerClient> _mealPlannerClientMock = null!;
        private GetShoppingListProductsQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _recipeRepoMock = new Mock<IRecipeRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<GetShoppingListProductsQueryHandler>>(MockBehavior.Loose);
            _mealPlannerClientMock = new Mock<IMealPlannerClient>(MockBehavior.Strict);

            _handler = new GetShoppingListProductsQueryHandler(
                _recipeRepoMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _mealPlannerClientMock.Object);
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_RecipeNotFound_ReturnsNull()
        {
            // Arrange
            var recipeId = Guid.NewGuid();
            var query = new GetShoppingListProductsQuery
            {
                RecipeId = recipeId,
                ShopId = Guid.NewGuid(),
                AuthToken = "token"
            };

            _recipeRepoMock
                .Setup(r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Recipe?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Null);
            _recipeRepoMock.Verify(
                r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()),
                Times.Once);
            _mealPlannerClientMock.Verify(
                c => c.GetShopAsync(It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_ShopNotFound_ReturnsNull()
        {
            // Arrange
            var recipeId = Guid.NewGuid();
            var shopId = Guid.NewGuid();
            var query = new GetShoppingListProductsQuery
            {
                RecipeId = recipeId,
                ShopId = shopId,
                AuthToken = "token"
            };

            var recipe = new Data.Entities.Recipe();

            _recipeRepoMock
                .Setup(r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(recipe);

            _mealPlannerClientMock
                .Setup(c => c.GetShopAsync(shopId, "token", It.IsAny<CancellationToken>()))
                .ReturnsAsync((ShopEditModel?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Null);
            _recipeRepoMock.Verify(
                r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()),
                Times.Once);
            _mealPlannerClientMock.Verify(
                c => c.GetShopAsync(shopId, "token", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Handle_RecipeAndShopFound_ReturnsMappedAndSortedProducts()
        {
            // Arrange
            var recipeId = Guid.NewGuid();
            var shopId = Guid.NewGuid();
            var query = new GetShoppingListProductsQuery
            {
                RecipeId = recipeId,
                ShopId = shopId,
                AuthToken = "token"
            };

            var unit = new Data.Entities.Unit()
            {
                Id = Guid.NewGuid(),
                Name = "kg",
                UnitType = Common.Constants.Units.UnitType.Weight
            };
            var recipe = new Data.Entities.Recipe()
            {
                RecipeIngredients =
                [
                    new RecipeIngredient()
                    {
                        ProductId = Guid.NewGuid(),
                        Product = new Data.Entities.Product ()
                        {
                            Id = Guid.NewGuid(),
                            Name = "B",
                            BaseUnit = unit
                        },
                        Quantity = 5,
                        Unit = unit
                    },
                    new RecipeIngredient()
                    {
                        ProductId = Guid.NewGuid(),
                        Product = new Data.Entities.Product ()
                        {
                            Id = Guid.NewGuid(),
                            Name = "A",
                            BaseUnit = unit
                        },
                        Quantity = 10,
                        Unit = unit
                    },
                ]
            };

            _recipeRepoMock
                .Setup(r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(recipe);

            var shopEditModel = new ShopEditModel
            {
                Id = shopId,
                Name = "Test",
            };

            _mealPlannerClientMock
                .Setup(c => c.GetShopAsync(shopId, "token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(shopEditModel);

            var shopEntity = new Shop();
            _mapperMock
                .Setup(m => m.Map<Shop>(shopEditModel))
                .Returns(shopEntity);

            _mapperMock
                .Setup(m => m.Map<ShoppingListProductEditModel>(It.IsAny<ShoppingListProduct>()))
                .Returns<ShoppingListProduct>(p => new ShoppingListProductEditModel
                {
                    Product = p.Product == null ? null : new ProductModel
                    {
                        Id = p.Product.Id,
                        Name = p.Product.Name!
                    },
                    Quantity = p.Quantity,
                    DisplaySequence = p.DisplaySequence,
                    Collected = p.Collected
                });

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result[0].Product!.Name, Is.EqualTo("A"));
                Assert.That(result[1].Product!.Name, Is.EqualTo("B"));
                Assert.That(result[0].Index, Is.EqualTo(1));
                Assert.That(result[1].Index, Is.EqualTo(2));
            }

            _recipeRepoMock.Verify(
                r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()),
                Times.Once);
            _mealPlannerClientMock.Verify(
                c => c.GetShopAsync(shopId, "token", It.IsAny<CancellationToken>()),
                Times.Once);
            _mapperMock.Verify(m => m.Map<Shop>(shopEditModel), Times.Once);
            _mapperMock.VerifyAll();
        }

        [Test]
        public async Task Handle_Exception_LogsError_AndReturnsNull()
        {
            // Arrange
            var recipeId = Guid.NewGuid();
            var query = new GetShoppingListProductsQuery
            {
                RecipeId = recipeId,
                ShopId = Guid.NewGuid(),
                AuthToken = "token"
            };

            _recipeRepoMock
                .Setup(r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Null);

            _recipeRepoMock.Verify(
                r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()),
                Times.Once);

            _loggerMock.Verify(
                l => l.Log(
                    It.Is<LogLevel>(ll => ll == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}