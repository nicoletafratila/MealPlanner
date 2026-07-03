using AutoMapper;
using Common.Services;
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
        private Mock<ICurrentUserService> _currentUserMock = null!;
        private Mock<ILogger<AddCommandHandler>> _loggerMock = null!;
        private AddCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IShopRepository>(MockBehavior.Strict);
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
        public async Task Handle_ExistingShop_ReturnsFailedResponse_AndDoesNotAdd()
        {
            // Arrange
            var model = new ShopEditModel { Id = Guid.Empty, Name = "Shop1" };
            var command = new AddCommand { Model = model };

            var existingShops = new List<MealPlanner.Data.Entities.Shop>
            {
                new() { Id = Guid.NewGuid(), Name = "Shop1", UserId = "user1" }
            };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingShops);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("This shop already exists."));

            _repoMock.Verify(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<MealPlanner.Data.Entities.Shop>(It.IsAny<ShopEditModel>()), Times.Never);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<MealPlanner.Data.Entities.Shop>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_NewShop_AddsAndReturnsSuccess()
        {
            // Arrange
            var model = new ShopEditModel { Id = Guid.Empty, Name = "NewShop" };
            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var mappedEntity = new MealPlanner.Data.Entities.Shop { Id = Guid.NewGuid(), Name = "NewShop" };

            _mapperMock
                .Setup(m => m.Map<MealPlanner.Data.Entities.Shop>(model))
                .Returns(mappedEntity);

            _repoMock
                .Setup(r => r.AddAsync(mappedEntity, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedEntity);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _repoMock.Verify(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<MealPlanner.Data.Entities.Shop>(model), Times.Once);
            _repoMock.Verify(r => r.AddAsync(mappedEntity, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_ExceptionDuringAdd_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            var model = new ShopEditModel { Id = Guid.Empty, Name = "ErrorShop" };
            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var mappedEntity = new MealPlanner.Data.Entities.Shop { Id = Guid.NewGuid(), Name = "ErrorShop" };

            _mapperMock
                .Setup(m => m.Map<MealPlanner.Data.Entities.Shop>(model))
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

            _repoMock.Verify(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<MealPlanner.Data.Entities.Shop>(model), Times.Once);
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
