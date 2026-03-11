using AutoMapper;
using MealPlanner.Api.Features.ShoppingList.Commands.Add;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace MealPlanner.Api.Tests.Features.ShoppingList.Commands.Add
{
    [TestFixture]
    public class AddCommandHandlerTests
    {
        private Mock<IShoppingListRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<ILogger<AddCommandHandler>> _loggerMock = null!;
        private AddCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IShoppingListRepository>(MockBehavior.Strict);
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
        public async Task Handle_ExistingList_ReturnsFailedResponse_AndDoesNotAdd()
        {
            // Arrange
            var model = new ShoppingListEditModel
            {
                Id = 0,
                Name = "Weekly"
            };

            var command = new AddCommand { Model = model };

            var existing = new Common.Data.Entities.ShoppingList { Id = 10, Name = "Weekly" };

            _repoMock
                .Setup(r => r.SearchAsync("Weekly", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("This shopping list already exists."));

            _repoMock.Verify(r => r.SearchAsync("Weekly", It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.ShoppingList>(It.IsAny<ShoppingListEditModel>()), Times.Never);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Common.Data.Entities.ShoppingList>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_NewList_AddsAndReturnsSuccess()
        {
            // Arrange
            var model = new ShoppingListEditModel
            {
                Id = 0,
                Name = "NewList"
            };

            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.SearchAsync("NewList", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Common.Data.Entities.ShoppingList?)null);

            var mappedEntity = new Common.Data.Entities.ShoppingList
            {
                Id = 5,
                Name = "NewList"
            };

            _mapperMock
                .Setup(m => m.Map<Common.Data.Entities.ShoppingList>(model))
                .Returns(mappedEntity);

            _repoMock
                .Setup(r => r.AddAsync(mappedEntity, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedEntity);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _repoMock.Verify(r => r.SearchAsync("NewList", It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.ShoppingList>(model), Times.Once);
            _repoMock.Verify(r => r.AddAsync(mappedEntity, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_ExceptionDuringAdd_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            var model = new ShoppingListEditModel
            {
                Id = 0,
                Name = "ErrorList"
            };

            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.SearchAsync("ErrorList", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Common.Data.Entities.ShoppingList?)null);

            var mappedEntity = new Common.Data.Entities.ShoppingList
            {
                Id = 7,
                Name = "ErrorList"
            };

            _mapperMock
                .Setup(m => m.Map<Common.Data.Entities.ShoppingList>(model))
                .Returns(mappedEntity);

            _repoMock
                .Setup(r => r.AddAsync(mappedEntity, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("An error occurred when saving the shopping list."));

            _repoMock.Verify(r => r.SearchAsync("ErrorList", It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.ShoppingList>(model), Times.Once);
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