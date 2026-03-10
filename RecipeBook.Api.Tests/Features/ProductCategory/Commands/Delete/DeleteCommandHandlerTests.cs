using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Features.ProductCategory.Commands.Delete;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Tests.Features.ProductCategory.Commands.Delete
{
    [TestFixture]
    public class DeleteCommandHandlerTests
    {
        private Mock<IProductCategoryRepository> _categoryRepoMock = null!;
        private Mock<IProductRepository> _productRepoMock = null!;
        private Mock<ILogger<DeleteCommandHandler>> _loggerMock = null!;
        private DeleteCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _categoryRepoMock = new Mock<IProductCategoryRepository>(MockBehavior.Strict);
            _productRepoMock = new Mock<IProductRepository>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<DeleteCommandHandler>>(MockBehavior.Loose);

            _handler = new DeleteCommandHandler(
                _categoryRepoMock.Object,
                _productRepoMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullCategoryRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new DeleteCommandHandler(null!, _productRepoMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullProductRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new DeleteCommandHandler(_categoryRepoMock.Object, null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new DeleteCommandHandler(_categoryRepoMock.Object, _productRepoMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_CategoryNotFound_ReturnsFailedResponse()
        {
            // Arrange
            const int id = 5;
            var command = new DeleteCommand { Id = id };

            _categoryRepoMock
                .Setup(r => r.GetByIdAsync(id, CancellationToken.None))
                .ReturnsAsync((Common.Data.Entities.ProductCategory?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Could not find with id 5."));
            }

            _categoryRepoMock.Verify(r => r.GetByIdAsync(id, CancellationToken.None), Times.Once);
            _productRepoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Never);
            _categoryRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Common.Data.Entities.ProductCategory>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task Handle_CategoryUsedInProducts_ReturnsFailedResponse_AndDoesNotDelete()
        {
            // Arrange
            const int id = 3;
            var command = new DeleteCommand { Id = id };

            var category = new Common.Data.Entities.ProductCategory { Id = id, Name = "Dairy" };

            var products = new List<Common.Data.Entities.Product>
            {
                new() { Id = 1, Name = "Milk", ProductCategoryId = id },
                new() { Id = 2, Name = "Bread", ProductCategoryId = 999 }
            };

            _categoryRepoMock
                .Setup(r => r.GetByIdAsync(id, CancellationToken.None))
                .ReturnsAsync(category);

            _productRepoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(products);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Product category 'Dairy' can not be deleted, it is used in products."));
            }

            _categoryRepoMock.Verify(r => r.GetByIdAsync(id, CancellationToken.None), Times.Once);
            _productRepoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _categoryRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Common.Data.Entities.ProductCategory>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task Handle_CategoryNotUsedInProducts_DeletesAndReturnsSuccess()
        {
            // Arrange
            const int id = 4;
            var command = new DeleteCommand { Id = id };

            var category = new Common.Data.Entities.ProductCategory { Id = id, Name = "Snacks" };

            var products = new List<Common.Data.Entities.Product>
            {
                new() { Id = 1, Name = "Milk", ProductCategoryId = 999 }
            };

            _categoryRepoMock
                .Setup(r => r.GetByIdAsync(id, CancellationToken.None))
                .ReturnsAsync(category);

            _productRepoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(products);

            _categoryRepoMock
                .Setup(r => r.DeleteAsync(category, CancellationToken.None))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _categoryRepoMock.Verify(r => r.GetByIdAsync(id, CancellationToken.None), Times.Once);
            _productRepoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _categoryRepoMock.Verify(r => r.DeleteAsync(category, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task Handle_ExceptionDuringDelete_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            const int id = 6;
            var command = new DeleteCommand { Id = id };

            var category = new Common.Data.Entities.ProductCategory { Id = id, Name = "Frozen" };

            var products = new List<Common.Data.Entities.Product>();

            _categoryRepoMock
                .Setup(r => r.GetByIdAsync(id, CancellationToken.None))
                .ReturnsAsync(category);

            _productRepoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(products);

            _categoryRepoMock
                .Setup(r => r.DeleteAsync(category, CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred when deleting the product category."));
            }

            _categoryRepoMock.Verify(r => r.GetByIdAsync(id, CancellationToken.None), Times.Once);
            _productRepoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _categoryRepoMock.Verify(r => r.DeleteAsync(category, CancellationToken.None), Times.Once);

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