using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Features.Product.Commands.Add;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.Product.Commands.Add
{
    [TestFixture]
    public class AddCommandHandlerTests
    {
        private Mock<IProductRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<ILogger<AddCommandHandler>> _loggerMock = null!;
        private AddCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IProductRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<AddCommandHandler>>(MockBehavior.Loose);

            _handler = new AddCommandHandler(
                _repoMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new AddCommandHandler(null!, _mapperMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new AddCommandHandler(_repoMock.Object, null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new AddCommandHandler(_repoMock.Object, _mapperMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public void Handle_NullModel_ThrowsArgumentNullException()
        {
            var command = new AddCommand { Model = null! };

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Test]
        public async Task Handle_ExistingProduct_ReturnsFailedResponse_AndDoesNotAdd()
        {
            // Arrange
            var model = new ProductEditModel
            {
                Id = 0,
                Name = "Milk",
                BaseUnitId = 1,
                ProductCategoryId = 2
            };

            var command = new AddCommand { Model = model };

            var existing = new Common.Data.Entities.Product { Id = 10, Name = "Milk", ProductCategoryId = 2 };

            _repoMock
                .Setup(r => r.SearchAsync("Milk"))
                .ReturnsAsync(existing);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("This product already exists."));
            }

            _repoMock.Verify(r => r.SearchAsync("Milk"), Times.Once);
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.Product>(It.IsAny<ProductEditModel>()), Times.Never);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Common.Data.Entities.Product>()), Times.Never);
        }

        [Test]
        public async Task Handle_NewProduct_AddsAndReturnsSuccess()
        {
            // Arrange
            var model = new ProductEditModel
            {
                Id = 0,
                Name = "Bread",
                BaseUnitId = 1,
                ProductCategoryId = 2
            };

            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.SearchAsync("Bread"))
                .ReturnsAsync((Common.Data.Entities.Product?)null);

            var mappedEntity = new Common.Data.Entities.Product
            {
                Id = 5,
                Name = "Bread",
                ProductCategoryId = 2,
                BaseUnitId = 1
            };

            _mapperMock
                .Setup(m => m.Map<Common.Data.Entities.Product>(model))
                .Returns(mappedEntity);

            _repoMock
                .Setup(r => r.AddAsync(mappedEntity))
                .ReturnsAsync(mappedEntity);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _repoMock.Verify(r => r.SearchAsync("Bread"), Times.Once);
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.Product>(model), Times.Once);
            _repoMock.Verify(r => r.AddAsync(mappedEntity), Times.Once);
        }

        [Test]
        public async Task Handle_ExceptionDuringAdd_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            var model = new ProductEditModel
            {
                Id = 0,
                Name = "ErrorProduct",
                BaseUnitId = 1,
                ProductCategoryId = 2
            };

            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.SearchAsync("ErrorProduct"))
                .ReturnsAsync((Common.Data.Entities.Product?)null);

            var mappedEntity = new Common.Data.Entities.Product
            {
                Id = 7,
                Name = "ErrorProduct",
                ProductCategoryId = 2,
                BaseUnitId = 1
            };

            _mapperMock
                .Setup(m => m.Map<Common.Data.Entities.Product>(model))
                .Returns(mappedEntity);

            _repoMock
                .Setup(r => r.AddAsync(mappedEntity))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred when saving the product."));
            }

            _repoMock.Verify(r => r.SearchAsync("ErrorProduct"), Times.Once);
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.Product>(model), Times.Once);
            _repoMock.Verify(r => r.AddAsync(mappedEntity), Times.Once);

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