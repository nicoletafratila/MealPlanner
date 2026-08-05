using MealPlanner.Api.Features.ShoppingList.Commands.UpdateProductCollected;
using MealPlanner.Api.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace MealPlanner.Api.Tests.Features.ShoppingList.Commands.UpdateProductCollected
{
    [TestFixture]
    public class UpdateProductCollectedCommandHandlerTests
    {
        private Mock<IShoppingListRepository> _repoMock = null!;
        private Mock<ILogger<UpdateProductCollectedCommandHandler>> _loggerMock = null!;
        private UpdateProductCollectedCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IShoppingListRepository>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<UpdateProductCollectedCommandHandler>>(MockBehavior.Loose);

            _handler = new UpdateProductCollectedCommandHandler(_repoMock.Object, _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new UpdateProductCollectedCommandHandler(null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new UpdateProductCollectedCommandHandler(_repoMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_RowUpdated_ReturnsSuccess()
        {
            // Arrange
            var shoppingListId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new UpdateProductCollectedCommand
            {
                ShoppingListId = shoppingListId,
                ProductId = productId,
                Collected = true
            };

            _repoMock
                .Setup(r => r.UpdateProductCollectedAsync(shoppingListId, productId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _repoMock.Verify(
                r => r.UpdateProductCollectedAsync(shoppingListId, productId, true, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Handle_NoRowMatched_ReturnsFailedResponse()
        {
            // Arrange
            var shoppingListId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new UpdateProductCollectedCommand
            {
                ShoppingListId = shoppingListId,
                ProductId = productId,
                Collected = false
            };

            _repoMock
                .Setup(r => r.UpdateProductCollectedAsync(shoppingListId, productId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo($"Could not find with id {shoppingListId}"));
            }
        }

        [Test]
        public async Task Handle_RepositoryThrows_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            var shoppingListId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new UpdateProductCollectedCommand
            {
                ShoppingListId = shoppingListId,
                ProductId = productId,
                Collected = true
            };

            _repoMock
                .Setup(r => r.UpdateProductCollectedAsync(shoppingListId, productId, true, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred when saving the shopping list."));
            }

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
