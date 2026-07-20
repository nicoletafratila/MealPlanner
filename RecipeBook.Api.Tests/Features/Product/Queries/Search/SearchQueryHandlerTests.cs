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
            _repoMock.Verify(r => r.GetAllByUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
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
            _repoMock.Verify(r => r.GetAllByUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _mapperMock.Verify(m => m.Map<IList<ProductModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoFiltersSortingOrCategory_MapsAndPaginatesAllResults()
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
                new() { Id = id1, Name = "P1", ProductCategoryId = "cat1" },
                new() { Id = id2, Name = "P2", ProductCategoryId = "cat2" }
            };

            _repoMock.Setup(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>())).ReturnsAsync(entities);
            _mapperMock.Setup(m => m.Map<IList<ProductModel>>(entities)).Returns(models);

            var qp = new QueryParameters<ProductModel> { Filters = null, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { CategoryId = null, QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items.Select(x => x.Name), Is.EquivalentTo(["P1", "P2"]));
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(2));
            }

            _repoMock.Verify(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ProductModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_CategoryFilter_AppliesFilter()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();
            var entities = new List<Data.Entities.Product>
            {
                new() { Id = id1, Name = "P1", ProductCategoryId = Guid.NewGuid() },
                new() { Id = id2, Name = "P2", ProductCategoryId = Guid.NewGuid() },
                new() { Id = id3, Name = "P3", ProductCategoryId = Guid.NewGuid() },
            };

            var models = new List<ProductModel>
            {
                new() { Id = id1, Name = "P1", ProductCategoryId = "10" },
                new() { Id = id2, Name = "P2", ProductCategoryId = "20" },
                new() { Id = id3, Name = "P3", ProductCategoryId = "10" },
            };

            _repoMock.Setup(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>())).ReturnsAsync(entities);
            _mapperMock.Setup(m => m.Map<IList<ProductModel>>(entities)).Returns(models);

            var qp = new QueryParameters<ProductModel> { Filters = null, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { CategoryId = "10", QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items, Has.Count.EqualTo(2));
            Assert.That(result.Items.Select(x => x.Name), Is.EquivalentTo(["P1", "P3"]));

            _repoMock.Verify(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ProductModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_HandledAsEmptyList()
        {
            var entities = new List<Data.Entities.Product>
            {
                new() { Id = Guid.NewGuid(), Name = "P1", ProductCategoryId = Guid.NewGuid() }
            };

            _repoMock.Setup(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>())).ReturnsAsync(entities);
            _mapperMock.Setup(m => m.Map<IList<ProductModel>?>(entities)).Returns((IList<ProductModel>?)null);

            var qp = new QueryParameters<ProductModel> { Filters = null, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { CategoryId = null, QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.Zero);
            }

            _repoMock.Verify(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ProductModel>>(entities), Times.Once);
        }
    }
}
