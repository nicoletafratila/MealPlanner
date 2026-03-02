using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Features.Unit.Commands.Update;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;
using UnitEntity = Common.Data.Entities.Unit;

namespace RecipeBook.Api.Tests.Features.Unit.Commands.Update
{
    [TestFixture]
    public class UpdateCommandHandlerTests
    {
        private Mock<IUnitRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<ILogger<UpdateCommandHandler>> _loggerMock = null!;
        private UpdateCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IUnitRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<UpdateCommandHandler>>(MockBehavior.Loose);

            _handler = new UpdateCommandHandler(
                _repoMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new UpdateCommandHandler(null!, _mapperMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new UpdateCommandHandler(_repoMock.Object, null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new UpdateCommandHandler(_repoMock.Object, _mapperMock.Object, null!));
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
            var command = new UpdateCommand { Model = null! };

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Test]
        public async Task Handle_EntityNotFound_ReturnsFailedResponse()
        {
            // Arrange
            var model = new UnitEditModel
            {
                Id = 10,
                Name = "kg",
                UnitType = Common.Constants.Units.UnitType.Weight
            };

            var command = new UpdateCommand { Model = model };

            _repoMock
                .Setup(r => r.GetByIdAsync(10))
                .ReturnsAsync((UnitEntity?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Does.StartWith("Could not find unit with id 10"));
            }

            _repoMock.Verify(r => r.GetByIdAsync(10), Times.Once);
            _mapperMock.Verify(m => m.Map(It.IsAny<UnitEditModel>(), It.IsAny<UnitEntity>()), Times.Never);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<UnitEntity>()), Times.Never);
        }

        [Test]
        public async Task Handle_SuccessfulUpdate_ReturnsSuccess()
        {
            // Arrange
            var model = new UnitEditModel
            {
                Id = 1,
                Name = "kg",
                UnitType = Common.Constants.Units.UnitType.Weight
            };

            var command = new UpdateCommand { Model = model };

            var existing = new UnitEntity
            {
                Id = 1,
                Name = "old",
                UnitType = Common.Constants.Units.UnitType.Weight
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existing);

            _mapperMock
               .Setup(m => m.Map(model, existing))
               .Returns(existing);

            _repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<UnitEntity>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _repoMock.Verify(r => r.GetByIdAsync(1), Times.Once);
            _mapperMock.Verify(m => m.Map(model, existing), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(existing), Times.Once);
        }

        [Test]
        public async Task Handle_ExceptionDuringUpdate_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            var model = new UnitEditModel
            {
                Id = 2,
                Name = "g",
                UnitType = Common.Constants.Units.UnitType.Weight
            };

            var command = new UpdateCommand { Model = model };

            var existing = new UnitEntity
            {
                Id = 2,
                Name = "old",
                UnitType = Common.Constants.Units.UnitType.Weight
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(existing);

            _mapperMock
                .Setup(m => m.Map(model, existing))
                .Returns(existing);

            _repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<UnitEntity>()))
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

            _repoMock.Verify(r => r.GetByIdAsync(2), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<UnitEntity>()), Times.Once);
            _mapperMock.Verify(m => m.Map(model, existing), Times.Once);

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