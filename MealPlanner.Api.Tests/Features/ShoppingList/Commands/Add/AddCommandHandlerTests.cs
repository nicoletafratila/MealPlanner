using AutoMapper;
using Common.Services;
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
        private Mock<ICurrentUserService> _currentUserMock = null!;
        private Mock<ILogger<AddCommandHandler>> _loggerMock = null!;
        private AddCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IShoppingListRepository>(MockBehavior.Strict);
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
        public async Task Handle_ExistingList_ReturnsFailedResponse_AndDoesNotAdd()
        {
            var model = new ShoppingListEditModel { Id = Guid.Empty, Name = "Weekly" };
            var command = new AddCommand { Model = model };
            var existing = new Data.Entities.ShoppingList { Id = Guid.NewGuid(), Name = "Weekly" };

            _repoMock
                .Setup(r => r.SearchAsync("Weekly", "user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("This shopping list already exists."));

            _repoMock.Verify(r => r.SearchAsync("Weekly", "user1", It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<Data.Entities.ShoppingList>(It.IsAny<ShoppingListEditModel>()), Times.Never);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Data.Entities.ShoppingList>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_NewList_AddsAndReturnsSuccess()
        {
            var model = new ShoppingListEditModel { Id = Guid.Empty, Name = "NewList" };
            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.SearchAsync("NewList", "user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.ShoppingList?)null);

            var mappedEntity = new Data.Entities.ShoppingList { Id = Guid.NewGuid(), Name = "NewList" };

            _mapperMock
                .Setup(m => m.Map<Data.Entities.ShoppingList>(model))
                .Returns(mappedEntity);

            _repoMock
                .Setup(r => r.AddAsync(mappedEntity, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedEntity);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _repoMock.Verify(r => r.SearchAsync("NewList", "user1", It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<Data.Entities.ShoppingList>(model), Times.Once);
            _repoMock.Verify(r => r.AddAsync(mappedEntity, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_NewList_SetsCreatedAtOnMappedEntity()
        {
            var model = new ShoppingListEditModel { Id = Guid.Empty, Name = "NewList" };
            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.SearchAsync("NewList", "user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.ShoppingList?)null);

            var mappedEntity = new Data.Entities.ShoppingList { Id = Guid.NewGuid(), Name = "NewList" };

            _mapperMock
                .Setup(m => m.Map<Data.Entities.ShoppingList>(model))
                .Returns(mappedEntity);

            _repoMock
                .Setup(r => r.AddAsync(mappedEntity, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedEntity);

            var before = DateTime.Now;
            var result = await _handler.Handle(command, CancellationToken.None);
            var after = DateTime.Now;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);
            Assert.That(mappedEntity.CreatedAt, Is.Not.Null);
            Assert.That(mappedEntity.CreatedAt, Is.InRange(before, after));
        }

        [Test]
        public async Task Handle_ExceptionDuringAdd_LogsError_AndReturnsFailedResponse()
        {
            var model = new ShoppingListEditModel { Id = Guid.Empty, Name = "ErrorList" };
            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.SearchAsync("ErrorList", "user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.ShoppingList?)null);

            var mappedEntity = new Data.Entities.ShoppingList { Id = Guid.NewGuid(), Name = "ErrorList" };

            _mapperMock
                .Setup(m => m.Map<Data.Entities.ShoppingList>(model))
                .Returns(mappedEntity);

            _repoMock
                .Setup(r => r.AddAsync(mappedEntity, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("An error occurred when saving the shopping list."));

            _repoMock.Verify(r => r.SearchAsync("ErrorList", "user1", It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<Data.Entities.ShoppingList>(model), Times.Once);
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
