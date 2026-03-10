using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Features.ProductCategory.Commands.Add;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.ProductCategory.Commands.Add
{
    [TestFixture]
    public class AddCommandHandlerTests
    {
        private Mock<IProductCategoryRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<ILogger<AddCommandHandler>> _loggerMock = null!;
        private AddCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IProductCategoryRepository>(MockBehavior.Strict);
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
        public async Task Handle_ExistingCategory_ReturnsFailedResponse_AndDoesNotAdd()
        {
            // Arrange
            var model = new ProductCategoryEditModel
            {
                Id = 0,
                Name = "Dairy"
            };

            var command = new AddCommand { Model = model };

            var existing = new List<Common.Data.Entities.ProductCategory>
            {
                new() { Id = 1, Name = "dairy" }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(existing);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("This product category already exists."));
            }

            _repoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.ProductCategory>(It.IsAny<ProductCategoryEditModel>()), Times.Never);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Common.Data.Entities.ProductCategory>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task Handle_NoExistingCategory_AddsAndReturnsSuccess()
        {
            // Arrange
            var model = new ProductCategoryEditModel
            {
                Id = 0,
                Name = "Snacks"
            };

            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync([]);

            var mappedEntity = new Common.Data.Entities.ProductCategory
            {
                Id = 5,
                Name = "Snacks"
            };

            _mapperMock
                .Setup(m => m.Map<Common.Data.Entities.ProductCategory>(model))
                .Returns(mappedEntity);

            _repoMock
                .Setup(r => r.AddAsync(mappedEntity, CancellationToken.None))
                .ReturnsAsync(mappedEntity);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _repoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.ProductCategory>(model), Times.Once);
            _repoMock.Verify(r => r.AddAsync(mappedEntity, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task Handle_ExceptionDuringAdd_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            var model = new ProductCategoryEditModel
            {
                Id = 0,
                Name = "ErrorCat"
            };

            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync([]);

            var mappedEntity = new Common.Data.Entities.ProductCategory
            {
                Id = 7,
                Name = "ErrorCat"
            };

            _mapperMock
                .Setup(m => m.Map<Common.Data.Entities.ProductCategory>(model))
                .Returns(mappedEntity);

            _repoMock
                .Setup(r => r.AddAsync(mappedEntity, CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred when saving the product category."));
            }

            _repoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.ProductCategory>(model), Times.Once);
            _repoMock.Verify(r => r.AddAsync(mappedEntity, CancellationToken.None), Times.Once);

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