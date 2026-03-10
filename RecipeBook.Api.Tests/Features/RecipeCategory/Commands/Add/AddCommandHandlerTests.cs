using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Features.RecipeCategory.Commands.Add;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.RecipeCategory.Commands.Add
{
    [TestFixture]
    public class AddCommandHandlerTests
    {
        private Mock<IRecipeCategoryRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<ILogger<AddCommandHandler>> _loggerMock = null!;
        private AddCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IRecipeCategoryRepository>(MockBehavior.Strict);
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
            var model = new RecipeCategoryEditModel
            {
                Id = 0,
                Name = "Breakfast",
                DisplaySequence = 1
            };

            var command = new AddCommand { Model = model };

            var existing = new List<Common.Data.Entities.RecipeCategory>
            {
                new() { Id = 1, Name = "breakfast", DisplaySequence = 1 }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(existing);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("This Recipe category already exists."));
            }

            _repoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.RecipeCategory>(It.IsAny<RecipeCategoryEditModel>()), Times.Never);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Common.Data.Entities.RecipeCategory>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task Handle_SuccessfulAdd_ReturnsSuccess()
        {
            // Arrange
            var model = new RecipeCategoryEditModel
            {
                Id = 0,
                Name = "Lunch",
                DisplaySequence = 2
            };

            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync([]);

            var mappedEntity = new Common.Data.Entities.RecipeCategory
            {
                Id = 5,
                Name = "Lunch",
                DisplaySequence = 2
            };

            _mapperMock
                .Setup(m => m.Map<Common.Data.Entities.RecipeCategory>(model))
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
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.RecipeCategory>(model), Times.Once);
            _repoMock.Verify(r => r.AddAsync(mappedEntity, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task Handle_ExceptionDuringAdd_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            var model = new RecipeCategoryEditModel
            {
                Id = 0,
                Name = "Dinner",
                DisplaySequence = 3
            };

            var command = new AddCommand { Model = model };

            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync([]);

            var mappedEntity = new Common.Data.Entities.RecipeCategory
            {
                Id = 7,
                Name = "Dinner",
                DisplaySequence = 3
            };

            _mapperMock
                .Setup(m => m.Map<Common.Data.Entities.RecipeCategory>(model))
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
                Assert.That(result.Message, Is.EqualTo("An error occurred when saving the Recipe category."));
            }

            _repoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _mapperMock.Verify(m => m.Map<Common.Data.Entities.RecipeCategory>(model), Times.Once);
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