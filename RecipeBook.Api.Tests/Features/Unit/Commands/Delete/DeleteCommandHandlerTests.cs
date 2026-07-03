using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Features.Unit.Commands.Delete;
using RecipeBook.Api.Repositories;
using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Tests.Features.Unit.Commands.Delete
{
    [TestFixture]
    public class DeleteCommandHandlerTests
    {
        private Mock<IUnitRepository> _unitRepoMock = null!;
        private Mock<IRecipeIngredientRepository> _ingredientRepoMock = null!;
        private Mock<ILogger<DeleteCommandHandler>> _loggerMock = null!;
        private DeleteCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _unitRepoMock = new Mock<IUnitRepository>(MockBehavior.Strict);
            _ingredientRepoMock = new Mock<IRecipeIngredientRepository>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<DeleteCommandHandler>>(MockBehavior.Loose);

            _handler = new DeleteCommandHandler(
                _unitRepoMock.Object,
                _ingredientRepoMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullUnitRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new DeleteCommandHandler(null!, _ingredientRepoMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullIngredientRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new DeleteCommandHandler(_unitRepoMock.Object, null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new DeleteCommandHandler(_unitRepoMock.Object, _ingredientRepoMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_UnitNotFound_ReturnsFailedResponse()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new DeleteCommand(id);

            _unitRepoMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((RecipeBook.Data.Entities.Unit?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Does.Contain(id.ToString()));
            }

            _unitRepoMock.Verify(
                r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()),
                Times.Once);
            _ingredientRepoMock.Verify(
                r => r.GetAllAsync(It.IsAny<CancellationToken>()),
                Times.Never);
            _unitRepoMock.Verify(
                r => r.DeleteAsync(It.IsAny<RecipeBook.Data.Entities.Unit>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_UnitUsedInIngredients_ReturnsFailedResponse_AndDoesNotDelete()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new DeleteCommand(id);

            var unit = new RecipeBook.Data.Entities.Unit
            {
                Id = id,
                Name = "kg",
                UnitType = Common.Constants.Units.UnitType.Weight
            };

            var ingredients = new List<RecipeIngredient>
            {
                new() { UnitId = id, Quantity = 1 },
                new() { UnitId = Guid.NewGuid(), Quantity = 2 }
            };

            _unitRepoMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(unit);

            _ingredientRepoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredients);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Unit kg can not be deleted, it is used in products."));
            }

            _unitRepoMock.Verify(
                r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()),
                Times.Once);
            _ingredientRepoMock.Verify(
                r => r.GetAllAsync(It.IsAny<CancellationToken>()),
                Times.Once);
            _unitRepoMock.Verify(
                r => r.DeleteAsync(It.IsAny<RecipeBook.Data.Entities.Unit>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_UnitNotUsedInIngredients_DeletesAndReturnsSuccess()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new DeleteCommand(id);

            var unit = new RecipeBook.Data.Entities.Unit
            {
                Id = id,
                Name = "g",
                UnitType = Common.Constants.Units.UnitType.Weight
            };

            var ingredients = new List<RecipeIngredient>
            {
                new() { UnitId = Guid.NewGuid(), Quantity = 1 }
            };

            _unitRepoMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(unit);

            _ingredientRepoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredients);

            _unitRepoMock
                .Setup(r => r.DeleteAsync(unit, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _unitRepoMock.Verify(
                r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()),
                Times.Once);
            _ingredientRepoMock.Verify(
                r => r.GetAllAsync(It.IsAny<CancellationToken>()),
                Times.Once);
            _unitRepoMock.Verify(
                r => r.DeleteAsync(unit, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Handle_ExceptionDuringDelete_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new DeleteCommand(id);

            var unit = new RecipeBook.Data.Entities.Unit
            {
                Id = id,
                Name = "ml",
                UnitType = Common.Constants.Units.UnitType.Volume
            };

            var ingredients = new List<RecipeIngredient>();

            _unitRepoMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(unit);

            _ingredientRepoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredients);

            _unitRepoMock
                .Setup(r => r.DeleteAsync(unit, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred when deleting the unit."));
            }

            _unitRepoMock.Verify(
                r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()),
                Times.Once);
            _ingredientRepoMock.Verify(
                r => r.GetAllAsync(It.IsAny<CancellationToken>()),
                Times.Once);
            _unitRepoMock.Verify(
                r => r.DeleteAsync(unit, It.IsAny<CancellationToken>()),
                Times.Once);

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
