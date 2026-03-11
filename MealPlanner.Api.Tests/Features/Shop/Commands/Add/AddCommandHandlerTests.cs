using AutoMapper;
using MealPlanner.Api.Features.Shop.Commands.Add;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace MealPlanner.Api.Tests.Features.Shop.Commands.Add
{
    [TestFixture]
    public class AddCommandHandlerTests
    {
        private Mock<IShopRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<ILogger<AddCommandHandler>> _loggerMock = null!;
        private AddCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IShopRepository>(MockBehavior.Strict);
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
        public async Task Handle_ExistingShop_ReturnsFailedResponse_AndDoesNotAdd()
        {
            // Arrange
            var model = new ShopEditModel
            {
                Id = 0,
                Name = "Shop1"
            };

            var command = new AddCommand { Model = model };

            var existingShops = new List<Common.Data.Entities.Shop>
            {
                new() { Id = 1, Name = "Shop1" }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingShops);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("This shop already exists."));

            _repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.Shop>(It.IsAny<ShopEditModel>()), Times.Never);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Common.Data.Entities.Shop>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_NewShop_AddsAndReturnsSuccess()
        {
            // Arrange
            var model = new ShopEditModel
            {
                Id = 0,
                Name = "NewShop"
            };

            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var mappedEntity = new Common.Data.Entities.Shop
            {
                Id = 5,
                Name = "NewShop"
            };

            _mapperMock
                .Setup(m => m.Map<Common.Data.Entities.Shop>(model))
                .Returns(mappedEntity);

            _repoMock
                .Setup(r => r.AddAsync(mappedEntity, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedEntity);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.Shop>(model), Times.Once);
            _repoMock.Verify(r => r.AddAsync(mappedEntity, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_ExceptionDuringAdd_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            var model = new ShopEditModel
            {
                Id = 0,
                Name = "ErrorShop"
            };

            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var mappedEntity = new Common.Data.Entities.Shop
            {
                Id = 7,
                Name = "ErrorShop"
            };

            _mapperMock
                .Setup(m => m.Map<Common.Data.Entities.Shop>(model))
                .Returns(mappedEntity);

            _repoMock
                .Setup(r => r.AddAsync(mappedEntity, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("An error occurred when saving the shop."));

            _repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.Shop>(model), Times.Once);
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