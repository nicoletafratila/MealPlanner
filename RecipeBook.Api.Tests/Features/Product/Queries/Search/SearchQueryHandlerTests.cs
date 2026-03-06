using AutoMapper;
using Common.Pagination;
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
        private SearchQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IProductRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _handler = new SearchQueryHandler(_repoMock.Object, _mapperMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new SearchQueryHandler(null!, _mapperMock.Object));
        }

        [Test]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new SearchQueryHandler(_repoMock.Object, null!));
        }

        [Test]
        public async Task Handle_NullRequest_ReturnsEmptyPagedList()
        {
            // Act
            var result = await _handler.Handle(null!, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.Zero);
            }
            _repoMock.Verify(r => r.GetAllAsync(), Times.Never);
            _mapperMock.Verify(m => m.Map<IList<ProductModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NullQueryParameters_ReturnsEmptyPagedList()
        {
            // Arrange
            var query = new SearchQuery
            {
                CategoryId = null,
                QueryParameters = null
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.Zero);
            }
            _repoMock.Verify(r => r.GetAllAsync(), Times.Never);
            _mapperMock.Verify(m => m.Map<IList<ProductModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoFiltersSortingOrCategory_MapsAndPaginatesAllResults()
        {
            // Arrange
            var entities = new List<Common.Data.Entities.Product>
            {
                new() { Id = 1, Name = "P1", ProductCategoryId = 10 },
                new() { Id = 2, Name = "P2", ProductCategoryId = 20 }
            };

            var models = new List<ProductModel>
            {
                new() { Id = 1, Name = "P1", ProductCategoryId = "10" },
                new() { Id = 2, Name = "P2", ProductCategoryId = "20" }
            };

            _repoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<ProductModel>>(entities))
                .Returns(models);

            var qp = new QueryParameters<ProductModel>
            {
                Filters = null,
                Sorting = null,
                PageNumber = 1,
                PageSize = 10
            };

            var query = new SearchQuery
            {
                CategoryId = null,
                QueryParameters = qp
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result.Items, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items.Select(x => x.Id), Is.EquivalentTo([1, 2]));
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(2));
            }

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ProductModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_CategoryFilter_AppliesFilter()
        {
            // Arrange
            var entities = new List<Common.Data.Entities.Product>
            {
                new() { Id = 1, Name = "P1", ProductCategoryId = 10 },
                new() { Id = 2, Name = "P2", ProductCategoryId = 20 },
                new() { Id = 3, Name = "P3", ProductCategoryId = 10 },
            };

            var models = new List<ProductModel>
            {
                new() { Id = 1, Name = "P1", ProductCategoryId = "10" },
                new() { Id = 2, Name = "P2", ProductCategoryId = "20" },
                new() { Id = 3, Name = "P3", ProductCategoryId = "10" },
            };

            _repoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<ProductModel>>(entities))
                .Returns(models);

            var qp = new QueryParameters<ProductModel>
            {
                Filters = null,
                Sorting = null,
                PageNumber = 1,
                PageSize = 10
            };

            var query = new SearchQuery
            {
                CategoryId = "10",
                QueryParameters = qp
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result.Items, Has.Count.EqualTo(2));
            Assert.That(result.Items.Select(x => x.Id), Is.EquivalentTo([1, 3]));

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ProductModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_HandledAsEmptyList()
        {
            // Arrange
            var entities = new List<Common.Data.Entities.Product>
            {
                new() { Id = 1, Name = "P1", ProductCategoryId = 10 }
            };

            _repoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<ProductModel>?>(entities))
                .Returns((IList<ProductModel>?)null);

            var qp = new QueryParameters<ProductModel>
            {
                Filters = null,
                Sorting = null,
                PageNumber = 1,
                PageSize = 10
            };

            var query = new SearchQuery
            {
                CategoryId = null,
                QueryParameters = qp
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.Zero);
            }

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ProductModel>>(entities), Times.Once);
        }
    }
}