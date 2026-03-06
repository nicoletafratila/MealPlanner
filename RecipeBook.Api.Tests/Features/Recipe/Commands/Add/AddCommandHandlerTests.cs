using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Features.Recipe.Commands.Add;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.Recipe.Commands.Add
{
    [TestFixture]
    public class AddCommandHandlerTests
    {
        private Mock<IRecipeRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<ILogger<AddCommandHandler>> _loggerMock = null!;
        private AddCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IRecipeRepository>(MockBehavior.Strict);
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
        public async Task Handle_ExistingRecipe_ReturnsFailedResponse_AndDoesNotAdd()
        {
            // Arrange
            var model = new RecipeEditModel
            {
                Id = 0,
                Name = "My Recipe",
                RecipeCategoryId = 1
            };

            var command = new AddCommand { Model = model };

            var existing = new Common.Data.Entities.Recipe { Id = 10, Name = "My Recipe", RecipeCategoryId = 1 };

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe"))
                .ReturnsAsync(existing);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("This recipe already exists in this category."));
            }

            _repoMock.Verify(r => r.SearchAsync("My Recipe"), Times.Once);
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.Recipe>(It.IsAny<RecipeEditModel>()), Times.Never);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Common.Data.Entities.Recipe>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoExistingRecipe_AddsAndReturnsSuccess()
        {
            // Arrange
            var model = new RecipeEditModel
            {
                Id = 0,
                Name = "New Recipe",
                RecipeCategoryId = 1
            };

            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.SearchAsync("New Recipe"))
                .ReturnsAsync((Common.Data.Entities.Recipe?)null);

            var mappedEntity = new Common.Data.Entities.Recipe
            {
                Id = 5,
                Name = "New Recipe",
                RecipeCategoryId = 1
            };

            _mapperMock
                .Setup(m => m.Map<Common.Data.Entities.Recipe>(model))
                .Returns(mappedEntity);

            _repoMock
                .Setup(r => r.AddAsync(mappedEntity))
                .ReturnsAsync(mappedEntity);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _repoMock.Verify(r => r.SearchAsync("New Recipe"), Times.Once);
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.Recipe>(model), Times.Once);
            _repoMock.Verify(r => r.AddAsync(mappedEntity), Times.Once);
        }

        [Test]
        public async Task Handle_ExceptionDuringAdd_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            var model = new RecipeEditModel
            {
                Id = 0,
                Name = "ErrorRecipe",
                RecipeCategoryId = 1
            };

            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.SearchAsync("ErrorRecipe"))
                .ReturnsAsync((Common.Data.Entities.Recipe?)null);

            var mappedEntity = new Common.Data.Entities.Recipe
            {
                Id = 7,
                Name = "ErrorRecipe",
                RecipeCategoryId = 1
            };

            _mapperMock
                .Setup(m => m.Map<Common.Data.Entities.Recipe>(model))
                .Returns(mappedEntity);

            _repoMock
                .Setup(r => r.AddAsync(mappedEntity))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred when saving the recipe."));
            }

            _repoMock.Verify(r => r.SearchAsync("ErrorRecipe"), Times.Once);
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.Recipe>(model), Times.Once);
            _repoMock.Verify(r => r.AddAsync(mappedEntity), Times.Once);

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