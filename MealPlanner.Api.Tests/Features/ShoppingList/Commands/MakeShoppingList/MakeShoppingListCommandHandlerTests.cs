using AutoMapper;
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
        private Mock<ILogger<MakeShoppingListCommandHandler>> _loggerMock = null!;
        private MakeShoppingListCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _mealPlanRepoMock = new Mock<IMealPlanRepository>(MockBehavior.Strict);
            _shoppingListRepoMock = new Mock<IShoppingListRepository>(MockBehavior.Strict);
            _shopRepoMock = new Mock<IShopRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<MakeShoppingListCommandHandler>>(MockBehavior.Loose);

            _handler = new MakeShoppingListCommandHandler(
                _mealPlanRepoMock.Object,
                _shoppingListRepoMock.Object,
                _shopRepoMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullDependencies_Throw()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new MakeShoppingListCommandHandler(
                    null!, _shoppingListRepoMock.Object, _shopRepoMock.Object, _mapperMock.Object, _loggerMock.Object));

            Assert.Throws<ArgumentNullException>(() =>
                _ = new MakeShoppingListCommandHandler(
                    _mealPlanRepoMock.Object, null!, _shopRepoMock.Object, _mapperMock.Object, _loggerMock.Object));

            Assert.Throws<ArgumentNullException>(() =>
                _ = new MakeShoppingListCommandHandler(
                    _mealPlanRepoMock.Object, _shoppingListRepoMock.Object, null!, _mapperMock.Object, _loggerMock.Object));

            Assert.Throws<ArgumentNullException>(() =>
                _ = new MakeShoppingListCommandHandler(
                    _mealPlanRepoMock.Object, _shoppingListRepoMock.Object, _shopRepoMock.Object, null!, _loggerMock.Object));

            Assert.Throws<ArgumentNullException>(() =>
                _ = new MakeShoppingListCommandHandler(
                    _mealPlanRepoMock.Object, _shoppingListRepoMock.Object, _shopRepoMock.Object, _mapperMock.Object, null!));
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
            var command = new MakeShoppingListCommand
            {
                MealPlanId = 1,
                ShopId = 2
            };

            _mealPlanRepoMock
                .Setup(r => r.GetByIdIncludeRecipesAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Common.Data.Entities.MealPlan?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Null);

            _mealPlanRepoMock.Verify(
                r => r.GetByIdIncludeRecipesAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
            _shopRepoMock.Verify(
                r => r.GetByIdIncludeDisplaySequenceAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _shoppingListRepoMock.Verify(
                r => r.AddAsync(It.IsAny<Common.Data.Entities.ShoppingList>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_ShopNotFound_ReturnsNull()
        {
            // Arrange
            var command = new MakeShoppingListCommand
            {
                MealPlanId = 1,
                ShopId = 2
            };

            var mealPlan = new Common.Data.Entities.MealPlan { Id = 1, Name = "Plan1" };

            _mealPlanRepoMock
                .Setup(r => r.GetByIdIncludeRecipesAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mealPlan);

            _shopRepoMock
                .Setup(r => r.GetByIdIncludeDisplaySequenceAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Common.Data.Entities.Shop?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Null);

            _mealPlanRepoMock.Verify(
                r => r.GetByIdIncludeRecipesAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
            _shopRepoMock.Verify(
                r => r.GetByIdIncludeDisplaySequenceAsync(2, It.IsAny<CancellationToken>()),
                Times.Once);
            _shoppingListRepoMock.Verify(
                r => r.AddAsync(It.IsAny<Common.Data.Entities.ShoppingList>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_SuccessfulFlow_ReturnsMappedEditModel()
        {
            // Arrange
            var command = new MakeShoppingListCommand
            {
                MealPlanId = 1,
                ShopId = 2
            };

            var mealPlan = new Common.Data.Entities.MealPlan { Id = 1, Name = "Plan1" };
            var shop = new Common.Data.Entities.Shop { Id = 2, Name = "Shop1" };
            var newList = new Common.Data.Entities.ShoppingList { Id = 10, Name = "Generated" };
            var loadedList = new Common.Data.Entities.ShoppingList { Id = 10, Name = "Generated", Products = [] };
            var mappedEdit = new ShoppingListEditModel { Id = 10, Name = "Generated" };

            _mealPlanRepoMock
                .Setup(r => r.GetByIdIncludeRecipesAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mealPlan);

            _shopRepoMock
                .Setup(r => r.GetByIdIncludeDisplaySequenceAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(shop);

            _shoppingListRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Common.Data.Entities.ShoppingList>(), It.IsAny<CancellationToken>()))
                .Callback<Common.Data.Entities.ShoppingList, CancellationToken>((s, _) =>
                {
                    Assert.That(s.Name, Is.EqualTo("Shopping list details for Plan1 in shop Shop1"));
                })
                .ReturnsAsync(newList);

            _shoppingListRepoMock
                .Setup(r => r.GetByIdIncludeProductsAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(loadedList);

            _mapperMock
                .Setup(m => m.Map<ShoppingListEditModel>(loadedList))
                .Returns(mappedEdit);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.Id, Is.EqualTo(10));
                Assert.That(result.Name, Is.EqualTo("Generated"));
            });

            _mealPlanRepoMock.Verify(
                r => r.GetByIdIncludeRecipesAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
            _shopRepoMock.Verify(
                r => r.GetByIdIncludeDisplaySequenceAsync(2, It.IsAny<CancellationToken>()),
                Times.Once);
            _shoppingListRepoMock.Verify(
                r => r.AddAsync(It.IsAny<Common.Data.Entities.ShoppingList>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _shoppingListRepoMock.Verify(
                r => r.GetByIdIncludeProductsAsync(10, It.IsAny<CancellationToken>()),
                Times.Once);
            _mapperMock.Verify(m => m.Map<ShoppingListEditModel>(loadedList), Times.Once);
        }

        [Test]
        public async Task Handle_Exception_LogsError_AndReturnsNull()
        {
            // Arrange
            var command = new MakeShoppingListCommand
            {
                MealPlanId = 1,
                ShopId = 2
            };

            _mealPlanRepoMock
                .Setup(r => r.GetByIdIncludeRecipesAsync(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Null);

            _mealPlanRepoMock.Verify(
                r => r.GetByIdIncludeRecipesAsync(1, It.IsAny<CancellationToken>()),
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