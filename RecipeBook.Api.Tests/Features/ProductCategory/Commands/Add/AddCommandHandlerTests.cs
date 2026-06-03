using AutoMapper;
using Common.Services;
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
        private Mock<ICurrentUserService> _currentUserMock = null!;
        private Mock<ILogger<AddCommandHandler>> _loggerMock = null!;
        private AddCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IProductCategoryRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _currentUserMock = new Mock<ICurrentUserService>(MockBehavior.Loose);
            _loggerMock = new Mock<ILogger<AddCommandHandler>>(MockBehavior.Loose);

            _currentUserMock.Setup(s => s.UserId).Returns("user1");

            _handler = new AddCommandHandler(
                _repoMock.Object,
                _mapperMock.Object,
                _currentUserMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new AddCommandHandler(null!, _mapperMock.Object, _currentUserMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new AddCommandHandler(_repoMock.Object, null!, _currentUserMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullCurrentUserService_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new AddCommandHandler(_repoMock.Object, _mapperMock.Object, null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new AddCommandHandler(_repoMock.Object, _mapperMock.Object, _currentUserMock.Object, null!));
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
            var model = new ProductCategoryEditModel
            {
                Id = Guid.Empty,
                Name = "Dairy"
            };

            var command = new AddCommand { Model = model };

            var existing = new List<RecipeBook.Data.Entities.ProductCategory>
            {
                new() { Id = Guid.NewGuid(), Name = "dairy" }
            };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("This product category already exists."));
            }

            _repoMock.Verify(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<RecipeBook.Data.Entities.ProductCategory>(It.IsAny<ProductCategoryEditModel>()), Times.Never);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<RecipeBook.Data.Entities.ProductCategory>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoExistingCategory_AddsAndReturnsSuccess()
        {
            var model = new ProductCategoryEditModel
            {
                Id = Guid.Empty,
                Name = "Snacks"
            };

            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var mappedEntity = new RecipeBook.Data.Entities.ProductCategory
            {
                Id = Guid.NewGuid(),
                Name = "Snacks"
            };

            _mapperMock
                .Setup(m => m.Map<RecipeBook.Data.Entities.ProductCategory>(model))
                .Returns(mappedEntity);

            _repoMock
                .Setup(r => r.AddAsync(mappedEntity, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedEntity);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _repoMock.Verify(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<RecipeBook.Data.Entities.ProductCategory>(model), Times.Once);
            _repoMock.Verify(r => r.AddAsync(mappedEntity, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_ExceptionDuringAdd_LogsError_AndReturnsFailedResponse()
        {
            var model = new ProductCategoryEditModel
            {
                Id = Guid.Empty,
                Name = "ErrorCat"
            };

            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var mappedEntity = new RecipeBook.Data.Entities.ProductCategory
            {
                Id = Guid.NewGuid(),
                Name = "ErrorCat"
            };

            _mapperMock
                .Setup(m => m.Map<RecipeBook.Data.Entities.ProductCategory>(model))
                .Returns(mappedEntity);

            _repoMock
                .Setup(r => r.AddAsync(mappedEntity, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred when saving the product category."));
            }

            _repoMock.Verify(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<RecipeBook.Data.Entities.ProductCategory>(model), Times.Once);
            _repoMock.Verify(r => r.AddAsync(mappedEntity, It.IsAny<CancellationToken>()), Times.Once);

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
