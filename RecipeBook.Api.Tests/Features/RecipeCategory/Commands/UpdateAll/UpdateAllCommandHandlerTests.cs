using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Features.RecipeCategory.Commands.UpdateAll;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.RecipeCategory.Commands.UpdateAll
{
    [TestFixture]
    public class UpdateAllCommandHandlerTests
    {
        private Mock<IRecipeCategoryRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<ILogger<UpdateAllCommandHandler>> _loggerMock = null!;
        private UpdateAllCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IRecipeCategoryRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<UpdateAllCommandHandler>>(MockBehavior.Loose);

            _handler = new UpdateAllCommandHandler(
                _repoMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new UpdateAllCommandHandler(null!, _mapperMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new UpdateAllCommandHandler(_repoMock.Object, null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new UpdateAllCommandHandler(_repoMock.Object, _mapperMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public void Handle_NullModels_ThrowsArgumentNullException()
        {
            var command = new UpdateAllCommand { Models = null! };

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Test]
        public async Task Handle_AllCategoriesFound_MapsAndUpdates_AndReturnsSuccess()
        {
            // Arrange
            var models = new List<RecipeCategoryModel>
            {
                new() { Id = 1, Name = "Cat1", DisplaySequence = 1 },
                new() { Id = 2, Name = "Cat2", DisplaySequence = 2 }
            };

            var command = new UpdateAllCommand { Models = models };

            var existing1 = new Common.Data.Entities.RecipeCategory { Id = 1, Name = "Old1", DisplaySequence = 10 };
            var existing2 = new Common.Data.Entities.RecipeCategory { Id = 2, Name = "Old2", DisplaySequence = 20 };

            _repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing1);

            _repoMock
                .Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing2);

            _mapperMock
                .Setup(m => m.Map(models[0], existing1))
                .Returns(existing1);

            _mapperMock
                .Setup(m => m.Map(models[1], existing2))
                .Returns(existing2);

            _repoMock
                .Setup(r => r.UpdateAllAsync(
                    It.Is<IList<Common.Data.Entities.RecipeCategory>>(list =>
                        list.Count == 2 &&
                        list.Contains(existing1) &&
                        list.Contains(existing2)),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _repoMock.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map(models[0], existing1), Times.Once);
            _mapperMock.Verify(m => m.Map(models[1], existing2), Times.Once);
            _repoMock.Verify(
                r => r.UpdateAllAsync(It.IsAny<IList<Common.Data.Entities.RecipeCategory>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Handle_SomeCategoriesMissing_ReturnsFailedResponse_AndDoesNotUpdate()
        {
            // Arrange
            var models = new List<RecipeCategoryModel>
            {
                new() { Id = 1, Name = "Cat1", DisplaySequence = 1 },
                new() { Id = 99, Name = "Missing", DisplaySequence = 3 }
            };

            var command = new UpdateAllCommand { Models = models };

            var existing1 = new Common.Data.Entities.RecipeCategory { Id = 1, Name = "Old1", DisplaySequence = 10 };

            _repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing1);

            _repoMock
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Common.Data.Entities.RecipeCategory?)null);

            _mapperMock
                .Setup(m => m.Map(models[0], existing1))
                .Returns(existing1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Does.Contain("Could not find with id 99"));
            }

            _repoMock.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map(models[0], existing1), Times.Once);
            _repoMock.Verify(
                r => r.UpdateAllAsync(It.IsAny<IList<Common.Data.Entities.RecipeCategory>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_NoItemsToUpdate_ReturnsSuccess_WithoutCallingUpdateAll()
        {
            // Arrange
            var models = new List<RecipeCategoryModel>(); // empty
            var command = new UpdateAllCommand { Models = models };

            // No repo calls expected for GetById/UpdateAll

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _repoMock.Verify(
                r => r.UpdateAllAsync(It.IsAny<IList<Common.Data.Entities.RecipeCategory>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_ExceptionDuringUpdate_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            var models = new List<RecipeCategoryModel>
            {
                new() { Id = 1, Name = "Cat1", DisplaySequence = 1 }
            };

            var command = new UpdateAllCommand { Models = models };

            var existing1 = new Common.Data.Entities.RecipeCategory { Id = 1, Name = "Old1", DisplaySequence = 10 };

            _repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing1);

            _mapperMock
                .Setup(m => m.Map(models[0], existing1))
                .Returns(existing1);

            _repoMock
                .Setup(r => r.UpdateAllAsync(It.IsAny<IList<Common.Data.Entities.RecipeCategory>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred when saving the Recipe category."));
            }

            _repoMock.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map(models[0], existing1), Times.Once);
            _repoMock.Verify(
                r => r.UpdateAllAsync(It.IsAny<IList<Common.Data.Entities.RecipeCategory>>(), It.IsAny<CancellationToken>()),
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