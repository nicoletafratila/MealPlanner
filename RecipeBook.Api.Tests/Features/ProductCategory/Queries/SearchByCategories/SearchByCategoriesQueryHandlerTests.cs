using AutoMapper;
using Common.Pagination;
using Moq;
using RecipeBook.Api.Features.ProductCategory.Queries.SearchByCategories;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.ProductCategory.Queries.SearchByCategories
{
    [TestFixture]
    public class SearchByCategoriesQueryHandlerTests
    {
        private Mock<IProductCategoryRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private SearchByCategoriesQueryHandler _handler = null!;

        private static Guid ProductCategoryGuid(int seed) => new(seed, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IProductCategoryRepository>(MockBehavior.Strict);
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
        public async Task Handle_NullRequest_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_EmptyCategoryIds_ReturnsAllCategories()
        {
            // Arrange
            var entities = new List<Data.Entities.ProductCategory>
            {
                new() { Id = ProductCategoryGuid(1), Name = "Cat1" },
                new() { Id = ProductCategoryGuid(2), Name = "Cat2" }
            };

            var models = new List<ProductCategoryModel>
            {
                new() { Id = ProductCategoryGuid(1), Name = "Cat1" },
                new() { Id = ProductCategoryGuid(2), Name = "Cat2" }
            };

            _repoMock
                .Setup(r => r.SearchByIdsAsync(
                    It.Is<IList<Guid>>(list => list.Count == 0),
                    null,
                    null,
                    CancellationToken.None))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<ProductCategoryModel>>(entities))
                .Returns(models);

            var query = new SearchByCategoriesQuery
            {
                CategoryIds = null
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(c => c.Id), Is.EquivalentTo(new[] { ProductCategoryGuid(1), ProductCategoryGuid(2) }));

            _mapperMock.Verify(m => m.Map<IList<ProductCategoryModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_WithCategoryIds_PassesIdsToRepository()
        {
            // Arrange
            var id1 = ProductCategoryGuid(1);
            var id3 = ProductCategoryGuid(3);

            var entities = new List<Data.Entities.ProductCategory>
            {
                new() { Id = id1, Name = "Cat1" },
                new() { Id = id3, Name = "Cat3" }
            };

            var models = new List<ProductCategoryModel>
            {
                new() { Id = id1, Name = "Cat1" },
                new() { Id = id3, Name = "Cat3" }
            };

            _repoMock
                .Setup(r => r.SearchByIdsAsync(
                    It.Is<IList<Guid>>(list => list.Count == 2 && list.Contains(id1) && list.Contains(id3)),
                    null,
                    null,
                    CancellationToken.None))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<ProductCategoryModel>>(entities))
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

            _mapperMock.VerifyAll();
        }

        [Test]
        public async Task Handle_InvalidCategoryIds_AreIgnored()
        {
            // Arrange
            var id1 = ProductCategoryGuid(1);

            var entities = new List<Data.Entities.ProductCategory>
            {
                new() { Id = id1, Name = "Cat1" }
            };

            var models = new List<ProductCategoryModel>
            {
                new() { Id = id1, Name = "Cat1" }
            };

            _repoMock
                .Setup(r => r.SearchByIdsAsync(
                    It.Is<IList<Guid>>(list => list.Count == 1 && list[0] == id1),
                    null,
                    null,
                    CancellationToken.None))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<ProductCategoryModel>>(entities))
                .Returns(models);

            var query = new SearchByCategoriesQuery
            {
                CategoryIds = $"{id1},xyz"
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(id1));

            _mapperMock.VerifyAll();
        }

        [Test]
        public async Task Handle_RepositoryReturnsEmpty_TreatedAsEmpty()
        {
            // Arrange
            _repoMock
                .Setup(r => r.SearchByIdsAsync(
                    It.Is<IList<Guid>>(list => list.Count == 0),
                    null,
                    null,
                    CancellationToken.None))
                .ReturnsAsync([]);

            _mapperMock
                .Setup(m => m.Map<IList<ProductCategoryModel>>(It.Is<IList<Data.Entities.ProductCategory>>(list => list.Count == 0)))
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

            _mapperMock.VerifyAll();
        }

        [Test]
        public async Task Handle_WithFiltersAndSorting_PassesThemToRepository()
        {
            // Arrange
            var id1 = ProductCategoryGuid(1);

            var entities = new List<Data.Entities.ProductCategory>
            {
                new() { Id = id1, Name = "Cat1" }
            };

            var models = new List<ProductCategoryModel>
            {
                new() { Id = id1, Name = "Cat1" }
            };

            var filters = new List<FilterItem> { new("Name", "Cat1", FilterOperator.Equals) };
            var sorting = new List<SortingModel> { new() { PropertyName = "Name" } };

            _repoMock
                .Setup(r => r.SearchByIdsAsync(
                    It.Is<IList<Guid>>(list => list.Count == 1 && list[0] == id1),
                    filters,
                    sorting,
                    CancellationToken.None))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<ProductCategoryModel>>(entities))
                .Returns(models);

            var query = new SearchByCategoriesQuery
            {
                CategoryIds = $"{id1}",
                Filters = filters,
                Sorting = sorting
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));

            _repoMock.VerifyAll();
            _mapperMock.VerifyAll();
        }
    }
}
