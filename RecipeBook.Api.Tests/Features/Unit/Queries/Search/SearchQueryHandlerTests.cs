using AutoMapper;
using Common.Pagination;
using Moq;
using RecipeBook.Api.Features.Unit.Queries.Search;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;
using UnitEntity = Common.Data.Entities.Unit;

namespace RecipeBook.Api.Tests.Features.Unit.Queries.Search
{
    [TestFixture]
    public class SearchQueryHandlerTests
    {
        private Mock<IUnitRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private SearchQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IUnitRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _handler = new SearchQueryHandler(_repoMock.Object, _mapperMock.Object);
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
            _mapperMock.Verify(m => m.Map<IList<UnitModel>>(It.IsAny<object>()), Times.Never);
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
            _repoMock.Verify(r => r.GetAllAsync(), Times.Never);
            _mapperMock.Verify(m => m.Map<IList<UnitModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoFiltersOrSorting_MapsAndPaginatesAllResults()
        {
            // Arrange
            var entities = new List<UnitEntity>
            {
                new() { Id = 1, Name = "kg", UnitType = Common.Constants.Units.UnitType.Weight },
                new() { Id = 2, Name = "g",  UnitType = Common.Constants.Units.UnitType.Weight }
            };

            var models = new List<UnitModel>
            {
                new() { Id = 1, Name = "kg", UnitType = Common.Constants.Units.UnitType.Weight },
                new() { Id = 2, Name = "g",  UnitType = Common.Constants.Units.UnitType.Weight }
            };

            _repoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<UnitModel>>(entities))
                .Returns(models);

            var qp = new QueryParameters<UnitModel>
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
                Assert.That(result.Items.Select(x => x.Name), Is.EquivalentTo(["kg", "g"]));
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(2));
            }

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<UnitModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_HandledAsEmptyList()
        {
            // Arrange
            var entities = new List<UnitEntity>
            {
                new() { Id = 1, Name = "kg", UnitType = Common.Constants.Units.UnitType.Weight }
            };

            _repoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<UnitModel>>(entities))
                .Returns([]);

            var qp = new QueryParameters<UnitModel>
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

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<UnitModel>>(entities), Times.Once);
        }
    }
}