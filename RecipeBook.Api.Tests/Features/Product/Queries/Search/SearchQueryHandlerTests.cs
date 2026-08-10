using AutoMapper;
using Common.Pagination;
using Common.Services;
using Moq;
using RecipeBook.Api.Features.Product.Queries.Search;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.Product.Queries.Search
{
    [TestFixture]
    public class SearchQueryHandlerTests
    {
        private Mock<IProductRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<ICurrentUserService> _currentUserMock = null!;
        private SearchQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IProductRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _currentUserMock = new Mock<ICurrentUserService>(MockBehavior.Loose);

            _currentUserMock.Setup(s => s.UserId).Returns("user1");

            _handler = new SearchQueryHandler(_repoMock.Object, _mapperMock.Object, _currentUserMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new SearchQueryHandler(null!, _mapperMock.Object, _currentUserMock.Object));
        }

        [Test]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new SearchQueryHandler(_repoMock.Object, null!, _currentUserMock.Object));
        }

        [Test]
        public void Ctor_NullCurrentUserService_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new SearchQueryHandler(_repoMock.Object, _mapperMock.Object, null!));
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
            _mapperMock.Verify(m => m.Map<IList<ProductModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NullQueryParameters_ReturnsEmptyPagedList()
        {
            var query = new SearchQuery { CategoryId = null, QueryParameters = null };

            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.Zero);
            }
            VerifySearchByUserAsyncNeverCalled();
            _mapperMock.Verify(m => m.Map<IList<ProductModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoFiltersSortingOrCategory_MapsPageAndSetsIndexes()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var entities = new List<Data.Entities.Product>
            {
                new() { Id = id1, Name = "P1", ProductCategoryId = Guid.NewGuid() },
                new() { Id = id2, Name = "P2", ProductCategoryId = Guid.NewGuid() }
            };

            var models = new List<ProductModel>
            {
                new() { Id = id1, Name = "P1" },
                new() { Id = id2, Name = "P2" }
            };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", null, null, null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.Product>(entities, 2, 0));
            _mapperMock.Setup(m => m.Map<IList<ProductModel>>(entities)).Returns(models);

            var qp = new QueryParameters<ProductModel> { Filters = null, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { CategoryId = null, QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Has.Count.EqualTo(2));
                Assert.That(result.Items.Select(x => x.Index), Is.EqualTo([1, 2]));
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(2));
            }

            _repoMock.Verify(r => r.SearchByUserAsync("user1", null, null, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ProductModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_ThumbnailOnlyTrue_PassesThumbnailOnlyToRepository()
        {
            var entities = new List<Data.Entities.Product> { new() { Id = Guid.NewGuid(), Name = "P1", ProductCategoryId = Guid.NewGuid() } };
            var models = new List<ProductModel> { new() { Name = "P1" } };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", null, null, null, 1, 10, It.IsAny<CancellationToken>(), true))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.Product>(entities, 1, 0));
            _mapperMock.Setup(m => m.Map<IList<ProductModel>>(entities)).Returns(models);

            var qp = new QueryParameters<ProductModel> { Filters = null, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { CategoryId = null, QueryParameters = qp, ThumbnailOnly = true };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items, Has.Count.EqualTo(1));
            _repoMock.Verify(r => r.SearchByUserAsync("user1", null, null, null, 1, 10, It.IsAny<CancellationToken>(), true), Times.Once);
        }

        [Test]
        public async Task Handle_ValidCategoryId_PassesParsedGuidToRepository()
        {
            var categoryId = Guid.NewGuid();
            var entities = new List<Data.Entities.Product>
            {
                new() { Id = Guid.NewGuid(), Name = "P1", ProductCategoryId = categoryId }
            };
            var models = new List<ProductModel> { new() { Name = "P1" } };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", categoryId, null, null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.Product>(entities, 1, 0));
            _mapperMock.Setup(m => m.Map<IList<ProductModel>>(entities)).Returns(models);

            var qp = new QueryParameters<ProductModel> { Filters = null, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { CategoryId = categoryId.ToString(), QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items, Has.Count.EqualTo(1));
            _repoMock.Verify(r => r.SearchByUserAsync("user1", categoryId, null, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_MalformedCategoryId_ReturnsEmptyWithoutCallingRepository()
        {
            var qp = new QueryParameters<ProductModel> { Filters = null, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { CategoryId = "not-a-guid", QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.Zero);
            }
            VerifySearchByUserAsyncNeverCalled();
        }

        [Test]
        public async Task Handle_WithFilters_PassesFiltersToRepositoryAndMapsResult()
        {
            var filters = new List<FilterItem> { new(nameof(ProductModel.Name), "Milk", FilterOperator.Equals) };
            var entities = new List<Data.Entities.Product> { new() { Id = Guid.NewGuid(), Name = "Milk" } };
            var models = new List<ProductModel> { new() { Name = "Milk" } };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", null, filters, null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.Product>(entities, 1, 0));
            _mapperMock.Setup(m => m.Map<IList<ProductModel>>(entities)).Returns(models);

            var qp = new QueryParameters<ProductModel> { Filters = filters, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { CategoryId = null, QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items.Single().Name, Is.EqualTo("Milk"));
            _repoMock.Verify(r => r.SearchByUserAsync("user1", null, filters, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_WithSorting_PassesSortingToRepositoryAndMapsResult()
        {
            var sorting = new List<SortingModel> { new() { PropertyName = nameof(ProductModel.Name), Direction = SortDirection.Ascending } };
            var entities = new List<Data.Entities.Product>
            {
                new() { Id = Guid.NewGuid(), Name = "Bread" },
                new() { Id = Guid.NewGuid(), Name = "Milk" }
            };
            var models = new List<ProductModel> { new() { Name = "Bread" }, new() { Name = "Milk" } };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", null, null, sorting, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.Product>(entities, 2, 0));
            _mapperMock.Setup(m => m.Map<IList<ProductModel>>(entities)).Returns(models);

            var qp = new QueryParameters<ProductModel> { Filters = null, Sorting = sorting, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { CategoryId = null, QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items.Select(x => x.Name), Is.EqualTo(["Bread", "Milk"]));
        }

        [Test]
        public async Task Handle_SecondPage_SetsIndexesFromSkip()
        {
            var entities = new List<Data.Entities.Product> { new() { Id = Guid.NewGuid(), Name = "P11" } };
            var models = new List<ProductModel> { new() { Name = "P11" } };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", null, null, null, 2, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.Product>(entities, 11, 10));
            _mapperMock.Setup(m => m.Map<IList<ProductModel>>(entities)).Returns(models);

            var qp = new QueryParameters<ProductModel> { Filters = null, Sorting = null, PageNumber = 2, PageSize = 10 };
            var query = new SearchQuery { CategoryId = null, QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items.Single().Index, Is.EqualTo(11));
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(11));
            }
        }

        [Test]
        public async Task Handle_MapperReturnsNull_HandledAsEmptyList()
        {
            var entities = new List<Data.Entities.Product>
            {
                new() { Id = Guid.NewGuid(), Name = "P1", ProductCategoryId = Guid.NewGuid() }
            };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", null, null, null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.Product>(entities, 1, 0));
            _mapperMock.Setup(m => m.Map<IList<ProductModel>?>(entities)).Returns((IList<ProductModel>?)null);

            var qp = new QueryParameters<ProductModel> { Filters = null, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { CategoryId = null, QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(1));
            }

            _repoMock.Verify(r => r.SearchByUserAsync("user1", null, null, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ProductModel>>(entities), Times.Once);
        }

        private void VerifySearchByUserAsyncNeverCalled() =>
            _repoMock.Verify(r => r.SearchByUserAsync(
                It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<IEnumerable<FilterItem>?>(),
                It.IsAny<IEnumerable<SortingModel>?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
    }
}
