using AutoMapper;
using Common.Pagination;
using MealPlanner.Api.Features.Shop.Queries.Search;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using Moq;

namespace MealPlanner.Api.Tests.Features.Shop.Queries.Search
{
    [TestFixture]
    public class SearchQueryHandlerTests
    {
        private Mock<IShopRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private SearchQueryHandler _handler = null!;
        private static readonly int[] expected = [1, 2];

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IShopRepository>(MockBehavior.Strict);
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

            // Assert
            Assert.That(result.Items, Is.Empty);
            Assert.That(result.Metadata.TotalCount, Is.EqualTo(0));
            _repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
            _mapperMock.Verify(m => m.Map<IList<ShopModel>>(It.IsAny<object>()), Times.Never);
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

            // Assert
            Assert.That(result.Items, Is.Empty);
            Assert.That(result.Metadata.TotalCount, Is.EqualTo(0));
            _repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
            _mapperMock.Verify(m => m.Map<IList<ShopModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoFiltersOrSorting_MapsAndPaginatesAllResults()
        {
            // Arrange
            var entities = new List<Common.Data.Entities.Shop>
            {
                new() { Id = 1, Name = "Shop1" },
                new() { Id = 2, Name = "Shop2" }
            };

            var models = new List<ShopModel>
            {
                new() { Id = 1, Name = "Shop1" },
                new() { Id = 2, Name = "Shop2" }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<ShopModel>>(entities))
                .Returns(models);

            var qp = new QueryParameters<ShopModel>
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
            Assert.That(result.Items.Count, Is.EqualTo(2));
            Assert.That(result.Items.Select(x => x.Id), Is.EquivalentTo(expected));
            Assert.That(result.Metadata.TotalCount, Is.EqualTo(2));

            _repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ShopModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_HandledAsEmptyList()
        {
            // Arrange
            var entities = new List<Common.Data.Entities.Shop>
            {
                new() { Id = 1, Name = "Shop1" }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<ShopModel>?>(entities))
                .Returns((IList<ShopModel>?)null);

            var qp = new QueryParameters<ShopModel>
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
            Assert.That(result.Items, Is.Empty);
            Assert.That(result.Metadata.TotalCount, Is.EqualTo(0));

            _repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ShopModel>>(entities), Times.Once);
        }
    }
}