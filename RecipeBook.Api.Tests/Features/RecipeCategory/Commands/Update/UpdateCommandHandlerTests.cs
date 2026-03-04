using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Features.RecipeCategory.Commands.Update;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.RecipeCategory.Commands.Update
{
    [TestFixture]
    public class UpdateCommandHandlerTests
    {
        private Mock<IRecipeCategoryRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<ILogger<UpdateCommandHandler>> _loggerMock = null!;
        private UpdateCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IRecipeCategoryRepository>(MockBehavior.Strict);
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
            const int id = 5;
            var model = new RecipeCategoryEditModel
            {
                Id = id,
                Name = "Breakfast",
                DisplaySequence = 1
            };

            var command = new UpdateCommand { Model = model };

            _repoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((Common.Data.Entities.RecipeCategory?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Could not find with id 5"));
            }

            _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
            _mapperMock.Verify(m => m.Map(It.IsAny<RecipeCategoryEditModel>(), It.IsAny<Common.Data.Entities.RecipeCategory>()), Times.Never);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Common.Data.Entities.RecipeCategory>()), Times.Never);
        }

        [Test]
        public async Task Handle_SuccessfulUpdate_ReturnsSuccess()
        {
            // Arrange
            const int id = 2;
            var model = new RecipeCategoryEditModel
            {
                Id = id,
                Name = "Lunch",
                DisplaySequence = 2
            };

            var command = new UpdateCommand { Model = model };

            var existing = new Common.Data.Entities.RecipeCategory      
            {
                Id = id,
                Name = "OldName",
                DisplaySequence = 10
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(existing);

            _mapperMock
                .Setup(m => m.Map(model, existing))
                .Returns(existing);

            _repoMock
                .Setup(r => r.UpdateAsync(existing))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
            _mapperMock.Verify(m => m.Map(model, existing), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(existing), Times.Once);
        }

        [Test]
        public async Task Handle_ExceptionDuringUpdate_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            const int id = 3;
            var model = new RecipeCategoryEditModel
            {
                Id = id,
                Name = "Dinner",
                DisplaySequence = 3
            };

            var command = new UpdateCommand { Model = model };

            var existing = new Common.Data.Entities.RecipeCategory
            {
                Id = id,
                Name = "OldDinner",
                DisplaySequence = 5
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(existing);

            _mapperMock
                .Setup(m => m.Map(model, existing))
                .Returns(existing);

            _repoMock
                .Setup(r => r.UpdateAsync(existing))
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

            _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
            _mapperMock.Verify(m => m.Map(model, existing), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(existing), Times.Once);

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