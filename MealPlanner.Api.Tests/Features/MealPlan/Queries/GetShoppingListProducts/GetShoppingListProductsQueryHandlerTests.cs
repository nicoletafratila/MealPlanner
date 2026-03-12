using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Api.Features.MealPlan.Queries.GetShoppingListProducts;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Tests.Features.MealPlan.Queries.GetShoppingListProducts
{
    [TestFixture]
    public class GetShoppingListProductsQueryHandlerTests
    {
        private Mock<IMealPlanRepository> _mealPlanRepoMock = null!;
        private Mock<IShopRepository> _shopRepoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<ILogger<GetShoppingListProductsQueryHandler>> _loggerMock = null!;
        private GetShoppingListProductsQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _mealPlanRepoMock = new Mock<IMealPlanRepository>(MockBehavior.Strict);
            _shopRepoMock = new Mock<IShopRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<GetShoppingListProductsQueryHandler>>(MockBehavior.Loose);

            _handler = new GetShoppingListProductsQueryHandler(
                _mealPlanRepoMock.Object,
                _shopRepoMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullDependencies_Throw()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new GetShoppingListProductsQueryHandler(null!, _shopRepoMock.Object, _mapperMock.Object, _loggerMock.Object));
            Assert.Throws<ArgumentNullException>(() =>
                _ = new GetShoppingListProductsQueryHandler(_mealPlanRepoMock.Object, null!, _mapperMock.Object, _loggerMock.Object));
            Assert.Throws<ArgumentNullException>(() =>
                _ = new GetShoppingListProductsQueryHandler(_mealPlanRepoMock.Object, _shopRepoMock.Object, null!, _loggerMock.Object));
            Assert.Throws<ArgumentNullException>(() =>
                _ = new GetShoppingListProductsQueryHandler(_mealPlanRepoMock.Object, _shopRepoMock.Object, _mapperMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_MealPlanNotFound_ReturnsNull()
        {
            // Arrange
            var query = new GetShoppingListProductsQuery { MealPlanId = 1, ShopId = 2 };

            _mealPlanRepoMock
                .Setup(r => r.GetByIdIncludeRecipesAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Common.Data.Entities.MealPlan?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Null);
            _mealPlanRepoMock.Verify(r => r.GetByIdIncludeRecipesAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _shopRepoMock.Verify(r => r.GetByIdIncludeDisplaySequenceAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_ShopNotFound_ReturnsNull()
        {
            // Arrange
            var query = new GetShoppingListProductsQuery { MealPlanId = 1, ShopId = 2 };

            var mealPlan = new Common.Data.Entities.MealPlan() { Id = 1, Name = "Plan1" };

            _mealPlanRepoMock
                .Setup(r => r.GetByIdIncludeRecipesAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mealPlan);

            _shopRepoMock
                .Setup(r => r.GetByIdIncludeDisplaySequenceAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Common.Data.Entities.Shop?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Null);
            _mealPlanRepoMock.Verify(r => r.GetByIdIncludeRecipesAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _shopRepoMock.Verify(r => r.GetByIdIncludeDisplaySequenceAsync(2, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_SuccessfulFlow_ReturnsSortedAndIndexedProducts()
        {
            // Arrange
            var query = new GetShoppingListProductsQuery { MealPlanId = 1, ShopId = 2 };

            var baseUnit = new Unit { Id = 1, Name = "kg" };
            var product1 = new Product
            {
                Id = 5,
                Name = "Flour",
                ProductCategory = new ProductCategory { Id = 100, Name = "Cat1" },
                BaseUnit = baseUnit
            };
            var product2 = new Product
            {
                Id = 6,
                Name = "Egg",
                ProductCategory = new ProductCategory { Id = 101, Name = "Cat2" },
                BaseUnit = baseUnit
            };
            var ingredient1 = new RecipeIngredient
            {
                ProductId = 5,
                Product = product1,
                Quantity = 2,
                Unit = baseUnit
            };
            var ingredient2 = new RecipeIngredient
            {
                ProductId = 6,
                Product = product2,
                Quantity = 4,
                Unit = baseUnit
            };
            var recipe = new Recipe
            {
                Name = "Cake",
                RecipeIngredients = [ingredient1, ingredient2]
            };
            var shop = new Common.Data.Entities.Shop { Id = 2, Name = "Shop1", DisplaySequence = [new ShopDisplaySequence() { Value = 2, ProductCategoryId = 100 }, new ShopDisplaySequence() { Value = 1, ProductCategoryId = 101 }] };
            var mealPlan = new Common.Data.Entities.MealPlan() { Id = 1, Name = "Plan1" };
            mealPlan.MealPlanRecipes =
            [
               new MealPlanRecipe()
               {
                   MealPlanId = mealPlan.Id,
                   Recipe = recipe
               }
            ];

            _mealPlanRepoMock
                .Setup(r => r.GetByIdIncludeRecipesAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mealPlan);

            _shopRepoMock
                .Setup(r => r.GetByIdIncludeDisplaySequenceAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(shop);

            _mapperMock
                .Setup(m => m.Map<ShoppingListProduct, ShoppingListProductEditModel>(
                    It.IsAny<ShoppingListProduct>()))
                .Returns((ShoppingListProduct src) =>
                {
                    return src.ProductId switch
                    {
                        5 => new ShoppingListProductEditModel
                        {
                            Collected = src.Collected,
                            DisplaySequence = src.DisplaySequence,
                            Quantity = ingredient1.Quantity,
                            Product = new ProductModel
                            {
                                Id = product1.Id,
                                Name = product1.Name!,
                            },
                        },
                        6 => new ShoppingListProductEditModel
                        {
                            Collected = src.Collected,
                            DisplaySequence = src.DisplaySequence,
                            Quantity = ingredient2.Quantity,
                            Product = new ProductModel
                            {
                                Id = product2.Id,
                                Name = product2.Name!,
                            },
                        }
                    };
                });

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2));

            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(result[0].Product!.Name, Is.EqualTo(product2.Name));
                    Assert.That(result[0].Quantity, Is.EqualTo(ingredient2.Quantity));
                    Assert.That(result[0].Index, Is.EqualTo(1));

                    Assert.That(result[1].Product!.Name, Is.EqualTo(product1.Name));
                    Assert.That(result[1].Quantity, Is.EqualTo(ingredient1.Quantity));
                    Assert.That(result[1].Index, Is.EqualTo(2));
                });
            }

            _mealPlanRepoMock.Verify(r => r.GetByIdIncludeRecipesAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _shopRepoMock.Verify(r => r.GetByIdIncludeDisplaySequenceAsync(2, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<ShoppingListProduct, ShoppingListProductEditModel>(It.IsAny<ShoppingListProduct>()), Times.Exactly(2));
        }

        [Test]
        public async Task Handle_Exception_LogsErrorAndReturnsNull()
        {
            // Arrange
            var query = new GetShoppingListProductsQuery { MealPlanId = 1, ShopId = 2 };

            _mealPlanRepoMock
                .Setup(r => r.GetByIdIncludeRecipesAsync(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Null);

            _mealPlanRepoMock.Verify(r => r.GetByIdIncludeRecipesAsync(1, It.IsAny<CancellationToken>()), Times.Once);

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