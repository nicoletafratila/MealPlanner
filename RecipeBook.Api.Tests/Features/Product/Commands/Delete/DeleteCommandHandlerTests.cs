using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Features.Product.Commands.Delete;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Tests.Features.Product.Commands.Delete
{
    [TestFixture]
    public class DeleteCommandHandlerTests
    {
        private Mock<IProductRepository> _productRepoMock = null!;
        private Mock<IRecipeIngredientRepository> _ingredientRepoMock = null!;
        private Mock<ILogger<DeleteCommandHandler>> _loggerMock = null!;
        private DeleteCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _productRepoMock = new Mock<IProductRepository>(MockBehavior.Strict);
            _ingredientRepoMock = new Mock<IRecipeIngredientRepository>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<DeleteCommandHandler>>(MockBehavior.Loose);

            _handler = new DeleteCommandHandler(
                _productRepoMock.Object,
                _ingredientRepoMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullProductRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new DeleteCommandHandler(null!, _ingredientRepoMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullIngredientRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new DeleteCommandHandler(_productRepoMock.Object, null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new DeleteCommandHandler(_productRepoMock.Object, _ingredientRepoMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_ProductNotFound_ReturnsFailedResponse()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new DeleteCommand { Id = id };

            _productRepoMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Product?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Does.Contain(id.ToString()));
            }

            _productRepoMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _ingredientRepoMock.Verify(r => r.SearchAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _productRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Data.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_ProductUsedInRecipes_ReturnsFailedResponse_AndDoesNotDelete()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new DeleteCommand { Id = id };
            var product = new Data.Entities.Product { Id = id, Name = "Milk" };
            var ingredients = new List<Data.Entities.RecipeIngredient>
            {
                new() { ProductId = id, Quantity = 1m }
            };

            _productRepoMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
            _ingredientRepoMock
                .Setup(r => r.SearchAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredients);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Product 'Milk' can not be deleted, it is used in recipes."));
            }

            _productRepoMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _ingredientRepoMock.Verify(r => r.SearchAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _productRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Data.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_ProductNotUsedInRecipes_DeletesAndReturnsSuccess()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new DeleteCommand { Id = id };
            var product = new Data.Entities.Product { Id = id, Name = "Bread" };

            _productRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
            _ingredientRepoMock.Setup(r => r.SearchAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync([]);
            _productRepoMock.Setup(r => r.DeleteAsync(product, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _productRepoMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _ingredientRepoMock.Verify(r => r.SearchAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _productRepoMock.Verify(r => r.DeleteAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_ExceptionDuringDelete_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new DeleteCommand { Id = id };
            var product = new Data.Entities.Product { Id = id, Name = "Cheese" };

            _productRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
            _ingredientRepoMock.Setup(r => r.SearchAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync([]);
            _productRepoMock.Setup(r => r.DeleteAsync(product, It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("DB error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred when deleting the product."));
            }

            _loggerMock.Verify(
                l => l.Log(It.Is<LogLevel>(ll => ll == LogLevel.Error), It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
