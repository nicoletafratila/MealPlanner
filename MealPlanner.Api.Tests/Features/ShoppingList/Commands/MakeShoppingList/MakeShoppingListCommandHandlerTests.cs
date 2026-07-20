using AutoMapper;
using Common.Services;
using MealPlanner.Api.Features.ShoppingList.Commands.MakeShoppingList;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace MealPlanner.Api.Tests.Features.ShoppingList.Commands.MakeShoppingList
{
    [TestFixture]
    public class MakeShoppingListCommandHandlerTests
    {
        private Mock<IMealPlanRepository> _mealPlanRepoMock = null!;
        private Mock<IShoppingListRepository> _shoppingListRepoMock = null!;
        private Mock<IShopRepository> _shopRepoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<ICurrentUserService> _currentUserMock = null!;
        private Mock<ILogger<MakeShoppingListCommandHandler>> _loggerMock = null!;
        private MakeShoppingListCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _mealPlanRepoMock = new Mock<IMealPlanRepository>(MockBehavior.Strict);
            _shoppingListRepoMock = new Mock<IShoppingListRepository>(MockBehavior.Strict);
            _shopRepoMock = new Mock<IShopRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _currentUserMock = new Mock<ICurrentUserService>(MockBehavior.Loose);
            _loggerMock = new Mock<ILogger<MakeShoppingListCommandHandler>>(MockBehavior.Loose);

            _currentUserMock.Setup(s => s.UserId).Returns("user1");

            _handler = new MakeShoppingListCommandHandler(
                _mealPlanRepoMock.Object,
                _shoppingListRepoMock.Object,
                _shopRepoMock.Object,
                _mapperMock.Object,
                _currentUserMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullDependencies_Throw()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new MakeShoppingListCommandHandler(
                    null!, _shoppingListRepoMock.Object, _shopRepoMock.Object, _mapperMock.Object, _currentUserMock.Object, _loggerMock.Object));

            Assert.Throws<ArgumentNullException>(() =>
                _ = new MakeShoppingListCommandHandler(
                    _mealPlanRepoMock.Object, null!, _shopRepoMock.Object, _mapperMock.Object, _currentUserMock.Object, _loggerMock.Object));

            Assert.Throws<ArgumentNullException>(() =>
                _ = new MakeShoppingListCommandHandler(
                    _mealPlanRepoMock.Object, _shoppingListRepoMock.Object, null!, _mapperMock.Object, _currentUserMock.Object, _loggerMock.Object));

            Assert.Throws<ArgumentNullException>(() =>
                _ = new MakeShoppingListCommandHandler(
                    _mealPlanRepoMock.Object, _shoppingListRepoMock.Object, _shopRepoMock.Object, null!, _currentUserMock.Object, _loggerMock.Object));

            Assert.Throws<ArgumentNullException>(() =>
                _ = new MakeShoppingListCommandHandler(
                    _mealPlanRepoMock.Object, _shoppingListRepoMock.Object, _shopRepoMock.Object, _mapperMock.Object, null!, _loggerMock.Object));

            Assert.Throws<ArgumentNullException>(() =>
                _ = new MakeShoppingListCommandHandler(
                    _mealPlanRepoMock.Object, _shoppingListRepoMock.Object, _shopRepoMock.Object, _mapperMock.Object, _currentUserMock.Object, null!));
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
            var mealPlanId = Guid.NewGuid();
            var command = new MakeShoppingListCommand { MealPlanId = mealPlanId, ShopId = Guid.NewGuid() };

            _mealPlanRepoMock
                .Setup(r => r.GetByIdIncludeRecipesAsync(mealPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.MealPlan?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Null);
            _mealPlanRepoMock.Verify(r => r.GetByIdIncludeRecipesAsync(mealPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _shopRepoMock.Verify(r => r.GetByIdIncludeDisplaySequenceAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
            _shoppingListRepoMock.Verify(r => r.AddAsync(It.IsAny<Data.Entities.ShoppingList>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_ShopNotFound_ReturnsNull()
        {
            var mealPlanId = Guid.NewGuid();
            var shopId = Guid.NewGuid();
            var command = new MakeShoppingListCommand { MealPlanId = mealPlanId, ShopId = shopId };
            var mealPlan = new Data.Entities.MealPlan { Id = mealPlanId, Name = "Plan1" };

            _mealPlanRepoMock
                .Setup(r => r.GetByIdIncludeRecipesAsync(mealPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mealPlan);

            _shopRepoMock
                .Setup(r => r.GetByIdIncludeDisplaySequenceAsync(shopId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Shop?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Null);
            _mealPlanRepoMock.Verify(r => r.GetByIdIncludeRecipesAsync(mealPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _shopRepoMock.Verify(r => r.GetByIdIncludeDisplaySequenceAsync(shopId, It.IsAny<CancellationToken>()), Times.Once);
            _shoppingListRepoMock.Verify(r => r.AddAsync(It.IsAny<Data.Entities.ShoppingList>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_SuccessfulFlow_ReturnsMappedEditModel()
        {
            var mealPlanId = Guid.NewGuid();
            var shopId = Guid.NewGuid();
            var command = new MakeShoppingListCommand { MealPlanId = mealPlanId, ShopId = shopId };
            var mealPlan = new Data.Entities.MealPlan { Id = mealPlanId, Name = "Plan1" };
            var shop = new Data.Entities.Shop { Id = shopId, Name = "Shop1" };
            var newListId = Guid.NewGuid();
            var newList = new Data.Entities.ShoppingList { Id = newListId, Name = "Generated", Products = [] };
            var mappedEdit = new ShoppingListEditModel { Id = newListId, Name = "Generated" };

            _mealPlanRepoMock
                .Setup(r => r.GetByIdIncludeRecipesAsync(mealPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mealPlan);

            _shopRepoMock
                .Setup(r => r.GetByIdIncludeDisplaySequenceAsync(shopId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(shop);

            _shoppingListRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.ShoppingList>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.ShoppingList, CancellationToken>((s, _) =>
                {
                    Assert.That(s.Name, Is.EqualTo("Shopping list details for Plan1 in shop Shop1"));
                })
                .ReturnsAsync(newList);

            _mapperMock
                .Setup(m => m.Map<ShoppingListEditModel>(newList))
                .Returns(mappedEdit);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Id, Is.EqualTo(newListId));
                Assert.That(result.Name, Is.EqualTo("Generated"));
            }

            _mealPlanRepoMock.Verify(r => r.GetByIdIncludeRecipesAsync(mealPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _shopRepoMock.Verify(r => r.GetByIdIncludeDisplaySequenceAsync(shopId, It.IsAny<CancellationToken>()), Times.Once);
            _shoppingListRepoMock.Verify(r => r.AddAsync(It.IsAny<Data.Entities.ShoppingList>(), It.IsAny<CancellationToken>()), Times.Once);
            _shoppingListRepoMock.Verify(r => r.GetByIdIncludeProductsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _mapperMock.Verify(m => m.Map<ShoppingListEditModel>(newList), Times.Once);
        }

        [Test]
        public async Task Handle_WithIngredients_RestoresProductAndUnitOnShoppingListProducts()
        {
            // Arrange
            var pieceUnit = new RecipeBook.Data.Entities.Unit { Id = Guid.NewGuid(), Name = "piece", UnitType = Common.Constants.Units.UnitType.Piece };
            var productId = Guid.NewGuid();
            var product = new RecipeBook.Data.Entities.Product
            {
                Id = productId,
                Name = "Milk",
                BaseUnitId = pieceUnit.Id,
                BaseUnit = pieceUnit
            };
            var ingredient = new RecipeBook.Data.Entities.RecipeIngredient
            {
                ProductId = productId,
                Product = product,
                Unit = pieceUnit,
                UnitId = pieceUnit.Id,
                Quantity = 2
            };
            var recipe = new RecipeBook.Data.Entities.Recipe
            {
                Id = Guid.NewGuid(),
                RecipeIngredients = [ingredient]
            };
            var mealPlanId = Guid.NewGuid();
            var mealPlan = new Data.Entities.MealPlan
            {
                Id = mealPlanId,
                Name = "Plan1",
                MealPlanRecipes = [new Data.Entities.MealPlanRecipe { RecipeId = recipe.Id, Recipe = recipe }]
            };
            var shopId = Guid.NewGuid();
            var shop = new Data.Entities.Shop { Id = shopId, Name = "Shop1" };

            var savedProduct = new Data.Entities.ShoppingListProduct { ProductId = productId };
            var savedListId = Guid.NewGuid();
            var savedList = new Data.Entities.ShoppingList
            {
                Id = savedListId,
                Name = "Plan1 Shop1",
                Products = [savedProduct]
            };
            var mappedEdit = new ShoppingListEditModel { Id = savedListId };

            _mealPlanRepoMock
                .Setup(r => r.GetByIdIncludeRecipesAsync(mealPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mealPlan);

            _shopRepoMock
                .Setup(r => r.GetByIdIncludeDisplaySequenceAsync(shopId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(shop);

            _shoppingListRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.ShoppingList>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(savedList);

            _mapperMock
                .Setup(m => m.Map<ShoppingListEditModel>(savedList))
                .Returns(mappedEdit);

            var command = new MakeShoppingListCommand { MealPlanId = mealPlanId, ShopId = shopId };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert ? nav props must be restored from the in-memory meal plan graph
            Assert.That(savedProduct.Product, Is.SameAs(product));
            Assert.That(savedProduct.Unit, Is.SameAs(pieceUnit));
        }

        [Test]
        public async Task Handle_Exception_LogsError_AndReturnsNull()
        {
            var mealPlanId = Guid.NewGuid();
            var command = new MakeShoppingListCommand { MealPlanId = mealPlanId, ShopId = Guid.NewGuid() };

            _mealPlanRepoMock
                .Setup(r => r.GetByIdIncludeRecipesAsync(mealPlanId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Null);
            _mealPlanRepoMock.Verify(r => r.GetByIdIncludeRecipesAsync(mealPlanId, It.IsAny<CancellationToken>()), Times.Once);

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
