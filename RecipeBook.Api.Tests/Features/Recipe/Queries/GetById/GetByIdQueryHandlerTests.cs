using AutoMapper;
using Moq;
using RecipeBook.Api.Features.Recipe.Queries.GetById;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.Recipe.Queries.GetById
{
    [TestFixture]
    public class GetByIdQueryHandlerTests
    {
        private Mock<IRecipeRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private GetByIdQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IRecipeRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _handler = new GetByIdQueryHandler(_repoMock.Object, _mapperMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new GetByIdQueryHandler(null!, _mapperMock.Object));
        }

        [Test]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new GetByIdQueryHandler(_repoMock.Object, null!));
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
            var entity = new Common.Data.Entities.Recipe
            {
                Id = id,
                Name = "My Recipe"
            };

            var mapped = new RecipeModel
            {
                Id = id,
                Name = "My Recipe"
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<RecipeModel>(entity))
                .Returns(mapped);

            var query = new GetByIdQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("My Recipe"));
            }

            _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
            _mapperMock.Verify(m => m.Map<RecipeModel>(entity), Times.Once);
        }

        [Test]
        public async Task Handle_EntityNotFound_ReturnsEmptyModelWithId()
        {
            // Arrange
            const int id = 10;

            _repoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((Common.Data.Entities.Recipe?)null);

            var query = new GetByIdQuery(id);

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
            _mapperMock.Verify(m => m.Map<RecipeModel>(It.IsAny<Common.Data.Entities.Recipe>()), Times.Never);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_FallsBackToEmptyModelWithId()
        {
            // Arrange
            const int id = 7;
            var entity = new Common.Data.Entities.Recipe
            {
                Id = id,
                Name = "Some Recipe"
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<RecipeModel?>(entity))
                .Returns((RecipeModel?)null);

            var query = new GetByIdQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(id));
            }

            _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
            _mapperMock.Verify(m => m.Map<RecipeModel>(entity), Times.Once);
        }
    }
}