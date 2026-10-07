using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Features.Recipe.Commands.Share;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Tests.Features.Recipe.Commands.Share
{
    [TestFixture]
    public class ShareCommandHandlerTests
    {
        private Mock<IRecipeRepository> _repoMock = null!;
        private Mock<ILogger<ShareCommandHandler>> _loggerMock = null!;
        private ShareCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IRecipeRepository>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<ShareCommandHandler>>(MockBehavior.Loose);

            _handler = new ShareCommandHandler(_repoMock.Object, _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareCommandHandler(null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareCommandHandler(_repoMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_RecipeNotFound_ReturnsFailedResponse_AndDoesNotAdd()
        {
            var recipeId = Guid.NewGuid();
            var command = new ShareCommand { RecipeId = recipeId, TargetUserId = "user2" };

            _repoMock
                .Setup(r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Recipe?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo($"Could not find with id {recipeId}"));
            }

            _repoMock.Verify(r => r.AddAsync(It.IsAny<Data.Entities.Recipe>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoNameCollision_SharesWithOriginalName()
        {
            var recipeId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var command = new ShareCommand { RecipeId = recipeId, TargetUserId = "user2" };

            var source = new Data.Entities.Recipe
            {
                Id = recipeId,
                Name = "My Recipe",
                Source = "cookbook",
                ImageContent = [1, 2, 3],
                ImageThumbnail = [4, 5, 6],
                RecipeCategoryId = categoryId,
                UserId = "user1",
                RecipeIngredients =
                [
                    new Data.Entities.RecipeIngredient { ProductId = Guid.NewGuid(), UnitId = Guid.NewGuid(), Quantity = 2 }
                ]
            };

            _repoMock
                .Setup(r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe", "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Recipe?)null);

            Data.Entities.Recipe? added = null;
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Recipe>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Recipe, CancellationToken>((r, _) => added = r)
                .ReturnsAsync((Data.Entities.Recipe r, CancellationToken _) => r);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            Assert.That(added, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(added!.Name, Is.EqualTo("My Recipe"));
                Assert.That(added.UserId, Is.EqualTo("user2"));
                Assert.That(added.Source, Is.EqualTo("cookbook"));
                Assert.That(added.RecipeCategoryId, Is.EqualTo(categoryId));
                Assert.That(added.RecipeIngredients, Has.Count.EqualTo(1));
                Assert.That(added.RecipeIngredients![0].ProductId, Is.EqualTo(source.RecipeIngredients![0].ProductId));
                Assert.That(added.RecipeIngredients![0].Quantity, Is.EqualTo(2));
            }

            _repoMock.Verify(r => r.SearchAsync("My Recipe", "user2", It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Data.Entities.Recipe>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_NameCollision_AppendsCopySuffix()
        {
            var recipeId = Guid.NewGuid();
            var command = new ShareCommand { RecipeId = recipeId, TargetUserId = "user2" };

            var source = new Data.Entities.Recipe
            {
                Id = recipeId,
                Name = "My Recipe",
                RecipeCategoryId = Guid.NewGuid(),
                UserId = "user1",
                RecipeIngredients = []
            };

            _repoMock
                .Setup(r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe", "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Data.Entities.Recipe { Id = Guid.NewGuid(), Name = "My Recipe" });

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe (Copy)", "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Recipe?)null);

            Data.Entities.Recipe? added = null;
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Recipe>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Recipe, CancellationToken>((r, _) => added = r)
                .ReturnsAsync((Data.Entities.Recipe r, CancellationToken _) => r);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(added!.Name, Is.EqualTo("My Recipe (Copy)"));

            _repoMock.Verify(r => r.SearchAsync("My Recipe", "user2", It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.SearchAsync("My Recipe (Copy)", "user2", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_RepeatedNameCollision_IncrementsCopySuffix()
        {
            var recipeId = Guid.NewGuid();
            var command = new ShareCommand { RecipeId = recipeId, TargetUserId = "user2" };

            var source = new Data.Entities.Recipe
            {
                Id = recipeId,
                Name = "My Recipe",
                RecipeCategoryId = Guid.NewGuid(),
                UserId = "user1",
                RecipeIngredients = []
            };

            _repoMock
                .Setup(r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe", "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Data.Entities.Recipe { Id = Guid.NewGuid(), Name = "My Recipe" });

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe (Copy)", "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Data.Entities.Recipe { Id = Guid.NewGuid(), Name = "My Recipe (Copy)" });

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe (Copy 2)", "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Recipe?)null);

            Data.Entities.Recipe? added = null;
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Recipe>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Recipe, CancellationToken>((r, _) => added = r)
                .ReturnsAsync((Data.Entities.Recipe r, CancellationToken _) => r);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(added!.Name, Is.EqualTo("My Recipe (Copy 2)"));
        }

        [Test]
        public async Task Handle_ExceptionDuringAdd_LogsError_AndReturnsFailedResponse()
        {
            var recipeId = Guid.NewGuid();
            var command = new ShareCommand { RecipeId = recipeId, TargetUserId = "user2" };

            var source = new Data.Entities.Recipe
            {
                Id = recipeId,
                Name = "My Recipe",
                RecipeCategoryId = Guid.NewGuid(),
                UserId = "user1",
                RecipeIngredients = []
            };

            _repoMock
                .Setup(r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe", "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Recipe?)null);

            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Recipe>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred when saving the recipe."));
            }

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
