using AutoMapper;
using Moq;
using RecipeBook.Api.Features.RecipeCategory.Queries.GetEdit;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.RecipeCategory.Queries.GetEdit
{
    [TestFixture]
    public class GetEditQueryHandlerTests
    {
        private Mock<IRecipeCategoryRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private GetEditQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IRecipeCategoryRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _handler = new GetEditQueryHandler(_repoMock.Object, _mapperMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new GetEditQueryHandler(null!, _mapperMock.Object));
        }

        [Test]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new GetEditQueryHandler(_repoMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_EntityFound_ReturnsMappedModel()
        {
            // Arrange
            const int id = 5;
            var entity = new Common.Data.Entities.RecipeCategory
            {
                Id = id,
                Name = "Breakfast",
                DisplaySequence = 1
            };

            var mapped = new RecipeCategoryEditModel
            {
                Id = id,
                Name = "Breakfast",
                DisplaySequence = 1
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<RecipeCategoryEditModel>(entity))
                .Returns(mapped);

            var query = new GetEditQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("Breakfast"));
            }

            _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
            _mapperMock.Verify(m => m.Map<RecipeCategoryEditModel>(entity), Times.Once);
        }

        [Test]
        public async Task Handle_EntityNotFound_ReturnsEmptyModelWithId()
        {
            // Arrange
            const int id = 10;

            _repoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((Common.Data.Entities.RecipeCategory?)null);

            var query = new GetEditQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.Null.Or.Empty);
            }

            _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
            _mapperMock.Verify(m => m.Map<RecipeCategoryEditModel>(It.IsAny<Common.Data.Entities.RecipeCategory>()), Times.Never);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_FallsBackToEmptyModelWithId()
        {
            // Arrange
            const int id = 7;
            var entity = new Common.Data.Entities.RecipeCategory
            {
                Id = id,
                Name = "Lunch",
                DisplaySequence = 2
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<RecipeCategoryEditModel>(entity))
                .Returns((RecipeCategoryEditModel)null);

            var query = new GetEditQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(id));
            }

            _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
            _mapperMock.Verify(m => m.Map<RecipeCategoryEditModel>(entity), Times.Once);
        }
    }
}