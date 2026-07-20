using AutoMapper;
using Moq;
using RecipeBook.Api.Features.RecipeCategory.Queries.SearchByCategories;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.RecipeCategory.Queries.SearchByCategories
{
    [TestFixture]
    public class SearchByCategoriesQueryHandlerTests
    {
        private Mock<IRecipeCategoryRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private SearchByCategoriesQueryHandler _handler = null!;

        private static Guid RecipeCategoryGuid(int seed) => new(seed, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IRecipeCategoryRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _handler = new SearchByCategoriesQueryHandler(_repoMock.Object, _mapperMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new SearchByCategoriesQueryHandler(null!, _mapperMock.Object));
        }

        [Test]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new SearchByCategoriesQueryHandler(_repoMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_EmptyCategoryIds_ReturnsAllCategories()
        {
            // Arrange
            var entities = new List<Data.Entities.RecipeCategory>
            {
                new() { Id = RecipeCategoryGuid(1), Name = "Cat1", DisplaySequence = 1 },
                new() { Id = RecipeCategoryGuid(2), Name = "Cat2", DisplaySequence = 2 }
            };

            var models = new List<RecipeCategoryModel>
            {
                new() { Id = RecipeCategoryGuid(1), Name = "Cat1", DisplaySequence = 1 },
                new() { Id = RecipeCategoryGuid(2), Name = "Cat2", DisplaySequence = 2 }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<RecipeCategoryModel>>(entities))
                .Returns(models);

            var query = new SearchByCategoriesQuery
            {
                CategoryIds = null
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(c => c.Id), Is.EquivalentTo(new[] { RecipeCategoryGuid(1), RecipeCategoryGuid(2) }));

            _repoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<RecipeCategoryModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_WithCategoryIds_FiltersCategoriesById()
        {
            // Arrange
            var id1 = RecipeCategoryGuid(1);
            var id3 = RecipeCategoryGuid(3);

            var entities = new List<Data.Entities.RecipeCategory>
            {
                new() { Id = id1, Name = "Cat1", DisplaySequence = 1 },
                new() { Id = RecipeCategoryGuid(2), Name = "Cat2", DisplaySequence = 2 },
                new() { Id = id3, Name = "Cat3", DisplaySequence = 3 }
            };

            var models = new List<RecipeCategoryModel>
            {
                new() { Id = id1, Name = "Cat1", DisplaySequence = 1 },
                new() { Id = id3, Name = "Cat3", DisplaySequence = 3 }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(entities);

            // The handler will pass the filtered list (ids 1 and 3) into Map
            _mapperMock
                .Setup(m => m.Map<IList<RecipeCategoryModel>>(
                    It.Is<IList<Data.Entities.RecipeCategory>>(list =>
                        list.Count == 2 &&
                        list.Any(e => e.Id == id1) &&
                        list.Any(e => e.Id == id3))))
                .Returns(models);

            var query = new SearchByCategoriesQuery
            {
                CategoryIds = $"{id1}, {id3}"
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(c => c.Id), Is.EquivalentTo(new[] { id1, id3 }));

            _repoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _mapperMock.VerifyAll();
        }

        [Test]
        public async Task Handle_InvalidCategoryIds_AreIgnored()
        {
            // Arrange
            var id1 = RecipeCategoryGuid(1);

            var entities = new List<Data.Entities.RecipeCategory>
            {
                new() { Id = id1, Name = "Cat1", DisplaySequence = 1 },
                new() { Id = RecipeCategoryGuid(2), Name = "Cat2", DisplaySequence = 2 }
            };

            var models = new List<RecipeCategoryModel>
            {
                new() { Id = id1, Name = "Cat1", DisplaySequence = 1 }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<RecipeCategoryModel>>(
                    It.Is<IList<Data.Entities.RecipeCategory>>(list =>
                        list.Count == 1 &&
                        list[0].Id == id1)))
                .Returns(models);

            var query = new SearchByCategoriesQuery
            {
                CategoryIds = $"{id1},abc"
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(id1));

            _repoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _mapperMock.VerifyAll();
        }

        [Test]
        public async Task Handle_RepositoryReturnsNull_TreatedAsEmpty()
        {
            // Arrange
            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync([]);

            _mapperMock
                .Setup(m => m.Map<IList<RecipeCategoryModel>>(
                    It.Is<IList<Data.Entities.RecipeCategory>>(list => list.Count == 0)))
                .Returns([]);

            var query = new SearchByCategoriesQuery
            {
                CategoryIds = "1,2"
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);

            _repoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _mapperMock.VerifyAll();
        }
    }
}