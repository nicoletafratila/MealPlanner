using AutoMapper;
using Common.Pagination;
using MealPlanner.Api.Features.ShoppingList.Queries.Search;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using Moq;

namespace MealPlanner.Api.Tests.Features.ShoppingList.Queries.Search
{
    [TestFixture]
    public class SearchQueryHandlerTests
    {
        private Mock<IShoppingListRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private SearchQueryHandler _handler = null!;
        private static readonly int[] expected = [1, 2];

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IShoppingListRepository>(MockBehavior.Strict);
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

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(0));
            });
            _repoMock.Verify(r => r.GetAllAsync(), Times.Never);
            _mapperMock.Verify(m => m.Map<IList<ShoppingListModel>>(It.IsAny<object>()), Times.Never);
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

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(0));
            });
            _repoMock.Verify(r => r.GetAllAsync(), Times.Never);
            _mapperMock.Verify(m => m.Map<IList<ShoppingListModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoFiltersOrSorting_MapsAndPaginatesAllResults()
        {
            // Arrange
            var entities = new List<Common.Data.Entities.ShoppingList>
            {
                new() { Id = 1, Name = "List1" },
                new() { Id = 2, Name = "List2" }
            };

            var models = new List<ShoppingListModel>
            {
                new() { Id = 1, Name = "List1" },
                new() { Id = 2, Name = "List2" }
            };

            _repoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<ShoppingListModel>>(entities))
                .Returns(models);

            var qp = new QueryParameters<ShoppingListModel>
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
            Assert.Multiple(() =>
            {
                Assert.That(result.Items.Select(x => x.Id), Is.EquivalentTo(expected));
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(2));
            });

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ShoppingListModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_HandledAsEmptyList()
        {
            // Arrange
            var entities = new List<Common.Data.Entities.ShoppingList>
            {
                new() { Id = 1, Name = "List1" }
            };

            _repoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<ShoppingListModel>?>(entities))
                .Returns((IList<ShoppingListModel>?)null);

            var qp = new QueryParameters<ShoppingListModel>
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

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(0));
            });

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ShoppingListModel>>(entities), Times.Once);
        }
    }
}