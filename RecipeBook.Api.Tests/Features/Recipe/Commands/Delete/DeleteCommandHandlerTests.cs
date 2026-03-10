using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Abstractions;
using RecipeBook.Api.Features.Recipe.Commands.Delete;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Tests.Features.Recipe.Commands.Delete
{
    [TestFixture]
    public class DeleteCommandHandlerTests
    {
        private Mock<IRecipeRepository> _repoMock = null!;
        private Mock<ILogger<DeleteCommandHandler>> _loggerMock = null!;
        private Mock<IMealPlannerClient> _mealPlannerClientMock = null!;
        private DeleteCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IRecipeRepository>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<DeleteCommandHandler>>(MockBehavior.Loose);
            _mealPlannerClientMock = new Mock<IMealPlannerClient>(MockBehavior.Strict);

            _handler = new DeleteCommandHandler(
                _repoMock.Object,
                _loggerMock.Object,
                _mealPlannerClientMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new DeleteCommandHandler(null!, _loggerMock.Object, _mealPlannerClientMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new DeleteCommandHandler(_repoMock.Object, null!, _mealPlannerClientMock.Object));
        }

        [Test]
        public void Ctor_NullMealPlannerClient_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new DeleteCommandHandler(_repoMock.Object, _loggerMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_RecipeNotFound_ReturnsFailedResponse()
        {
            // Arrange
            const int id = 5;
            var command = new DeleteCommand { Id = id, AuthToken = "token" };

            _repoMock
                .Setup(r => r.GetByIdAsync(id, CancellationToken.None))
                .ReturnsAsync((Common.Data.Entities.Recipe?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Could not find with id 5"));
            }

            _repoMock.Verify(r => r.GetByIdAsync(id, CancellationToken.None), Times.Once);
            _mealPlannerClientMock.Verify(c => c.GetMealPlansByRecipeIdAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
            _repoMock.Verify(r => r.DeleteAsync(It.IsAny<Common.Data.Entities.Recipe>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task Handle_RecipeUsedInMealPlans_ReturnsFailedResponse_AndDoesNotDelete()
        {
            // Arrange
            const int id = 5;
            var command = new DeleteCommand { Id = id, AuthToken = "token" };

            var recipe = new Common.Data.Entities.Recipe { Id = id, Name = "MyRecipe" };

            _repoMock
                .Setup(r => r.GetByIdAsync(id, CancellationToken.None))
                .ReturnsAsync(recipe);

            _mealPlannerClientMock
                .Setup(c => c.GetMealPlansByRecipeIdAsync(id, "token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                [
                    new() { Id = 1, Name = "Plan1" }
                ]);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Recipe MyRecipe can not be deleted, it is used in meal plans."));
            }

            _repoMock.Verify(r => r.GetByIdAsync(id, CancellationToken.None), Times.Once);
            _mealPlannerClientMock.Verify(c => c.GetMealPlansByRecipeIdAsync(id, "token", It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.DeleteAsync(It.IsAny<Common.Data.Entities.Recipe>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task Handle_RecipeNotUsedInMealPlans_DeletesAndReturnsSuccess()
        {
            // Arrange
            const int id = 5;
            var command = new DeleteCommand { Id = id, AuthToken = "token" };

            var recipe = new Common.Data.Entities.Recipe { Id = id, Name = "MyRecipe" };

            _repoMock
                .Setup(r => r.GetByIdAsync(id, CancellationToken.None))
                .ReturnsAsync(recipe);

            _mealPlannerClientMock
                .Setup(c => c.GetMealPlansByRecipeIdAsync(id, "token", It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            _repoMock
                .Setup(r => r.DeleteAsync(recipe, CancellationToken.None))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _repoMock.Verify(r => r.GetByIdAsync(id, CancellationToken.None), Times.Once);
            _mealPlannerClientMock.Verify(c => c.GetMealPlansByRecipeIdAsync(id, "token", It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.DeleteAsync(recipe, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task Handle_ExceptionDuringDelete_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            const int id = 5;
            var command = new DeleteCommand { Id = id, AuthToken = "token" };

            var recipe = new Common.Data.Entities.Recipe { Id = id, Name = "MyRecipe" };

            _repoMock
                .Setup(r => r.GetByIdAsync(id, CancellationToken.None))
                .ReturnsAsync(recipe);

            _mealPlannerClientMock
                .Setup(c => c.GetMealPlansByRecipeIdAsync(id, "token", It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            _repoMock
                .Setup(r => r.DeleteAsync(recipe, CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred when deleting the recipe."));
            }

            _repoMock.Verify(r => r.GetByIdAsync(id, CancellationToken.None), Times.Once);
            _mealPlannerClientMock.Verify(c => c.GetMealPlansByRecipeIdAsync(id, "token", It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.DeleteAsync(recipe, CancellationToken.None), Times.Once);

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