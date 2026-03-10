using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Features.Unit.Commands.Add;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.Unit.Commands.Add
{
    [TestFixture]
    public class AddCommandHandlerTests
    {
        private Mock<IUnitRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<ILogger<AddCommandHandler>> _loggerMock = null!;
        private AddCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IUnitRepository>(MockBehavior.Strict);
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
        public async Task Handle_DuplicateName_ReturnsFailedResponse_AndDoesNotAdd()
        {
            // Arrange
            var model = new UnitEditModel
            {
                Id = 0,
                Name = "kg",
                UnitType = Common.Constants.Units.UnitType.Weight
            };

            var command = new AddCommand { Model = model };

            var existingUnits = new List<Common.Data.Entities.Unit>
            {
                new() { Id = 1, Name = "Kg", UnitType = Common.Constants.Units.UnitType.Weight }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(existingUnits);

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
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.Unit>(It.IsAny<UnitEditModel>()), Times.Never);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Common.Data.Entities.Unit>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task Handle_SuccessfulAdd_ReturnsSuccess()
        {
            // Arrange
            var model = new UnitEditModel
            {
                Id = 0,
                Name = "kg",
                UnitType = Common.Constants.Units.UnitType.Weight
            };

            var command = new AddCommand { Model = model };

            var existingUnits = new List<Common.Data.Entities.Unit>(); // no duplicates

            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(existingUnits);

            var mappedEntity = new Common.Data.Entities.Unit
            {
                Id = 1,
                Name = "kg",
                UnitType = Common.Constants.Units.UnitType.Weight
            };

            _mapperMock
                .Setup(m => m.Map<Common.Data.Entities.Unit>(model))
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
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.Unit>(model), Times.Once);
            _repoMock.Verify(r => r.AddAsync(mappedEntity, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task Handle_ExceptionDuringAdd_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            var model = new UnitEditModel
            {
                Id = 0,
                Name = "g",
                UnitType = Common.Constants.Units.UnitType.Weight
            };

            var command = new AddCommand { Model = model };

            var existingUnits = new List<Common.Data.Entities.Unit>();

            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(existingUnits);

            var mappedEntity = new Common.Data.Entities.Unit
            {
                Id = 2,
                Name = "g",
                UnitType = Common.Constants.Units.UnitType.Weight
            };

            _mapperMock
                .Setup(m => m.Map<Common.Data.Entities.Unit>(model))
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
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.Unit>(model), Times.Once);
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