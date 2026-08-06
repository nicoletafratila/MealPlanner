using AutoMapper;
using Common.Pagination;
using Common.Services;
using Moq;
using RecipeBook.Api.Features.ProductCategory.Queries.Search;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.ProductCategory.Queries.Search
{
    [TestFixture]
    public class SearchQueryHandlerTests
    {
        private Mock<IProductCategoryRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<ICurrentUserService> _currentUserMock = null!;
        private SearchQueryHandler _handler = null!;

        private static Guid ProductCategoryGuid(int seed) => new(seed, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IProductCategoryRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _currentUserMock = new Mock<ICurrentUserService>(MockBehavior.Loose);

            _currentUserMock.Setup(s => s.UserId).Returns("user1");

            _handler = new SearchQueryHandler(_repoMock.Object, _mapperMock.Object, _currentUserMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new SearchQueryHandler(null!, _mapperMock.Object, _currentUserMock.Object));
        }

        [Test]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new SearchQueryHandler(_repoMock.Object, null!, _currentUserMock.Object));
        }

        [Test]
        public void Ctor_NullCurrentUserService_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new SearchQueryHandler(_repoMock.Object, _mapperMock.Object, null!));
        }

        [Test]
        public async Task Handle_NullRequest_ReturnsEmptyPagedList()
        {
            var result = await _handler.Handle(null!, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.Zero);
            }
            VerifySearchByUserAsyncNeverCalled();
            _mapperMock.Verify(m => m.Map<IList<ProductCategoryModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NullQueryParameters_ReturnsEmptyPagedList()
        {
            var query = new SearchQuery { QueryParameters = null };

            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.Zero);
            }
            VerifySearchByUserAsyncNeverCalled();
            _mapperMock.Verify(m => m.Map<IList<ProductCategoryModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoFiltersOrSorting_MapsPageAndSetsIndexes()
        {
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
                .Setup(r => r.SearchByUserAsync("user1", null, null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.ProductCategory>(entities, 2, 0));
            _mapperMock.Setup(m => m.Map<IList<ProductCategoryModel>>(entities)).Returns(models);

            var qp = new QueryParameters<ProductCategoryModel> { Filters = null, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Has.Count.EqualTo(2));
                Assert.That(result.Items.Select(x => x.Index), Is.EqualTo([1, 2]));
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(2));
            }

            _repoMock.Verify(r => r.SearchByUserAsync("user1", null, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ProductCategoryModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_WithFilters_PassesFiltersToRepositoryAndMapsResult()
        {
            var filters = new List<FilterItem> { new(nameof(ProductCategoryModel.Name), "Cat1", FilterOperator.Equals) };
            var entities = new List<Data.Entities.ProductCategory> { new() { Id = ProductCategoryGuid(1), Name = "Cat1" } };
            var models = new List<ProductCategoryModel> { new() { Id = ProductCategoryGuid(1), Name = "Cat1" } };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", filters, null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.ProductCategory>(entities, 1, 0));
            _mapperMock.Setup(m => m.Map<IList<ProductCategoryModel>>(entities)).Returns(models);

            var qp = new QueryParameters<ProductCategoryModel> { Filters = filters, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items.Single().Name, Is.EqualTo("Cat1"));
        }

        [Test]
        public async Task Handle_WithSorting_PassesSortingToRepositoryAndMapsResult()
        {
            var sorting = new List<SortingModel> { new() { PropertyName = nameof(ProductCategoryModel.Name), Direction = SortDirection.Ascending } };
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
                .Setup(r => r.SearchByUserAsync("user1", null, sorting, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.ProductCategory>(entities, 2, 0));
            _mapperMock.Setup(m => m.Map<IList<ProductCategoryModel>>(entities)).Returns(models);

            var qp = new QueryParameters<ProductCategoryModel> { Filters = null, Sorting = sorting, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items.Select(x => x.Name), Is.EqualTo(["Cat1", "Cat2"]));
        }

        [Test]
        public async Task Handle_MapperReturnsNull_HandledAsEmptyList()
        {
            var entities = new List<Data.Entities.ProductCategory> { new() { Id = ProductCategoryGuid(1), Name = "Cat1" } };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", null, null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.ProductCategory>(entities, 1, 0));
            _mapperMock.Setup(m => m.Map<IList<ProductCategoryModel>?>(entities)).Returns((IList<ProductCategoryModel>?)null);

            var qp = new QueryParameters<ProductCategoryModel> { Filters = null, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(1));
            }

            _repoMock.Verify(r => r.SearchByUserAsync("user1", null, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ProductCategoryModel>>(entities), Times.Once);
        }

        private void VerifySearchByUserAsyncNeverCalled() =>
            _repoMock.Verify(r => r.SearchByUserAsync(
                It.IsAny<string>(), It.IsAny<IEnumerable<FilterItem>?>(),
                It.IsAny<IEnumerable<SortingModel>?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
    }
}
