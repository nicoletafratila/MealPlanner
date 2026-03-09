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
            const int id = 5;
            var command = new DeleteCommand { Id = id };

            _productRepoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((Common.Data.Entities.Product?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Could not find with id 5"));
            }

            _productRepoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
            _ingredientRepoMock.Verify(r => r.SearchAsync(It.IsAny<int>()), Times.Never);
            _productRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Common.Data.Entities.Product>()), Times.Never);
        }

        [Test]
        public async Task Handle_ProductUsedInRecipes_ReturnsFailedResponse_AndDoesNotDelete()
        {
            // Arrange
            const int id = 3;
            var command = new DeleteCommand { Id = id };

            var product = new Common.Data.Entities.Product { Id = id, Name = "Milk" };

            var ingredients = new List<Common.Data.Entities.RecipeIngredient>
            {
                new() { ProductId = id, Quantity = 1m }
            };

            _productRepoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(product);

            _ingredientRepoMock
                .Setup(r => r.SearchAsync(id))
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

            _productRepoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
            _ingredientRepoMock.Verify(r => r.SearchAsync(id), Times.Once);
            _productRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Common.Data.Entities.Product>()), Times.Never);
        }

        [Test]
        public async Task Handle_ProductNotUsedInRecipes_DeletesAndReturnsSuccess()
        {
            // Arrange
            const int id = 4;
            var command = new DeleteCommand { Id = id };

            var product = new Common.Data.Entities.Product { Id = id, Name = "Bread" };

            _productRepoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(product);

            _ingredientRepoMock
                .Setup(r => r.SearchAsync(id))
                .ReturnsAsync([]);

            _productRepoMock
                .Setup(r => r.DeleteAsync(product))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _productRepoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
            _ingredientRepoMock.Verify(r => r.SearchAsync(id), Times.Once);
            _productRepoMock.Verify(r => r.DeleteAsync(product), Times.Once);
        }

        [Test]
        public async Task Handle_ExceptionDuringDelete_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            const int id = 6;
            var command = new DeleteCommand { Id = id };

            var product = new Common.Data.Entities.Product { Id = id, Name = "Cheese" };

            _productRepoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(product);

            _ingredientRepoMock
                .Setup(r => r.SearchAsync(id))
                .ReturnsAsync([]);

            _productRepoMock
                .Setup(r => r.DeleteAsync(product))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred when deleting the product."));
            }

            _productRepoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
            _ingredientRepoMock.Verify(r => r.SearchAsync(id), Times.Once);
            _productRepoMock.Verify(r => r.DeleteAsync(product), Times.Once);

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