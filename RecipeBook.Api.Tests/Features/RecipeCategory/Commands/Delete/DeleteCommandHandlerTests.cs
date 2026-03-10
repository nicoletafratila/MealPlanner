using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RecipeBook.Api.Features.RecipeCategory.Commands.Delete;
using RecipeBook.Api.Repositories;
using RecipeCategoryEntity = Common.Data.Entities.RecipeCategory;
using RecipeEntity = Common.Data.Entities.Recipe;

namespace RecipeBook.Api.Tests.Features.RecipeCategory.Commands.Delete
{
    [TestFixture]
    public class DeleteCommandHandlerTests
    {
        private Mock<IRecipeCategoryRepository> _categoryRepoMock = null!;
        private Mock<IRecipeRepository> _recipeRepoMock = null!;
        private Mock<ILogger<DeleteCommandHandler>> _loggerMock = null!;
        private DeleteCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _categoryRepoMock = new Mock<IRecipeCategoryRepository>(MockBehavior.Strict);
            _recipeRepoMock = new Mock<IRecipeRepository>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<DeleteCommandHandler>>(MockBehavior.Loose);

            _handler = new DeleteCommandHandler(
                _categoryRepoMock.Object,
                _recipeRepoMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullCategoryRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new DeleteCommandHandler(null!, _recipeRepoMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullRecipeRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new DeleteCommandHandler(_categoryRepoMock.Object, null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new DeleteCommandHandler(_categoryRepoMock.Object, _recipeRepoMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_CategoryNotFound_ReturnsFailedResponse()
        {
            // Arrange
            const int id = 10;
            var command = new DeleteCommand(id);

            _categoryRepoMock
                .Setup(r => r.GetByIdAsync(id, CancellationToken.None))
                .ReturnsAsync((RecipeCategoryEntity?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Could not find with id 10."));
            }

            _categoryRepoMock.Verify(r => r.GetByIdAsync(id, CancellationToken.None), Times.Once);
            _recipeRepoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Never);
            _categoryRepoMock.Verify(r => r.DeleteAsync(It.IsAny<RecipeCategoryEntity>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task Handle_CategoryUsedInRecipes_ReturnsFailedResponse_AndDoesNotDelete()
        {
            // Arrange
            const int id = 2;
            var command = new DeleteCommand(id);

            var category = new RecipeCategoryEntity { Id = id, Name = "Breakfast" };
            var recipes = new List<RecipeEntity>
            {
                new() { Id = 1, Name = "R1", RecipeCategoryId = id },
                new() { Id = 2, Name = "R2", RecipeCategoryId = 999 }
            };

            _categoryRepoMock
                .Setup(r => r.GetByIdAsync(id, CancellationToken.None))
                .ReturnsAsync(category);

            _recipeRepoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(recipes);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Recipe category Breakfast can not be deleted, it is used in recipes."));
            }

            _categoryRepoMock.Verify(r => r.GetByIdAsync(id, CancellationToken.None), Times.Once);
            _recipeRepoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _categoryRepoMock.Verify(r => r.DeleteAsync(It.IsAny<RecipeCategoryEntity>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task Handle_CategoryNotUsedInRecipes_DeletesAndReturnsSuccess()
        {
            // Arrange
            const int id = 3;
            var command = new DeleteCommand(id);

            var category = new RecipeCategoryEntity { Id = id, Name = "Lunch" };
            var recipes = new List<RecipeEntity>
            {
                new() { Id = 1, Name = "R1", RecipeCategoryId = 999 }
            };

            _categoryRepoMock
                .Setup(r => r.GetByIdAsync(id, CancellationToken.None))
                .ReturnsAsync(category);

            _recipeRepoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(recipes);

            _categoryRepoMock
                .Setup(r => r.DeleteAsync(category, CancellationToken.None))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _categoryRepoMock.Verify(r => r.GetByIdAsync(id, CancellationToken.None), Times.Once);
            _recipeRepoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _categoryRepoMock.Verify(r => r.DeleteAsync(category, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task Handle_ExceptionDuringDelete_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            const int id = 4;
            var command = new DeleteCommand(id);

            var category = new RecipeCategoryEntity { Id = id, Name = "Dinner" };
            var recipes = new List<RecipeEntity>(); // none referencing this category

            _categoryRepoMock
                .Setup(r => r.GetByIdAsync(id, CancellationToken.None))
                .ReturnsAsync(category);

            _recipeRepoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(recipes);

            _categoryRepoMock
                .Setup(r => r.DeleteAsync(category, CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred when deleting the recipe category."));
            }

            _categoryRepoMock.Verify(r => r.GetByIdAsync(id, CancellationToken.None), Times.Once);
            _recipeRepoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _categoryRepoMock.Verify(r => r.DeleteAsync(category, CancellationToken.None), Times.Once);

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