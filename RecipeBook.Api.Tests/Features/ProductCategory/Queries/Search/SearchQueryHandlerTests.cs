using AutoMapper;
using Common.Pagination;
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
        private SearchQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IProductCategoryRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _handler = new SearchQueryHandler(_repoMock.Object, _mapperMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
                _ = new SearchQueryHandler(null!, _mapperMock.Object));
        }

        [Test]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
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
            _repoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Never);
            _mapperMock.Verify(m => m.Map<IList<ProductCategoryModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NullQueryParameters_ReturnsEmptyPagedList()
        {
            // Arrange
            var query = new SearchQuery
            {
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
            _repoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Never);
            _mapperMock.Verify(m => m.Map<IList<ProductCategoryModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoFiltersOrSorting_MapsAndPaginatesAllResults()
        {
            // Arrange
            var entities = new List<Common.Data.Entities.ProductCategory>
            {
                new() { Id = 1, Name = "Cat1" },
                new() { Id = 2, Name = "Cat2" }
            };

            var models = new List<ProductCategoryModel>
            {
                new() { Id = 1, Name = "Cat1" },
                new() { Id = 2, Name = "Cat2" }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<ProductCategoryModel>>(entities))
                .Returns(models);

            var qp = new QueryParameters<ProductCategoryModel>
            {
                Filters = null,
                Sorting = null,
                PageNumber = 1,
                PageSize = 10
            };

            var query = new SearchQuery
            {
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

            _repoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ProductCategoryModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_HandledAsEmptyList()
        {
            // Arrange
            var entities = new List<Common.Data.Entities.ProductCategory>
            {
                new() { Id = 1, Name = "Cat1" }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<ProductCategoryModel>?>(entities))
                .Returns((IList<ProductCategoryModel>?)null);

            var qp = new QueryParameters<ProductCategoryModel>
            {
                Filters = null,
                Sorting = null,
                PageNumber = 1,
                PageSize = 10
            };

            var query = new SearchQuery
            {
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

            _repoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ProductCategoryModel>>(entities), Times.Once);
        }
    }
}