using AutoMapper;
using Common.Pagination;
using Common.Services;
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
        private Mock<ICurrentUserService> _currentUserMock = null!;
        private SearchQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IShoppingListRepository>(MockBehavior.Strict);
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
            _mapperMock.Verify(m => m.Map<IList<ShoppingListModel>>(It.IsAny<object>()), Times.Never);
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
            _mapperMock.Verify(m => m.Map<IList<ShoppingListModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoFiltersOrSorting_MapsPageAndSetsIndexes()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var entities = new List<Data.Entities.ShoppingList>
            {
                new() { Id = id1, Name = "List1" },
                new() { Id = id2, Name = "List2" }
            };

            var models = new List<ShoppingListModel>
            {
                new() { Id = id1, Name = "List1" },
                new() { Id = id2, Name = "List2" }
            };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", null, null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.ShoppingList>(entities, 2, 0));
            _mapperMock.Setup(m => m.Map<IList<ShoppingListModel>>(entities)).Returns(models);

            var qp = new QueryParameters<ShoppingListModel> { Filters = null, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Has.Count.EqualTo(2));
                Assert.That(result.Items.Select(x => x.Index), Is.EqualTo([1, 2]));
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(2));
            }

            _repoMock.Verify(r => r.SearchByUserAsync("user1", null, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ShoppingListModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_WithFilters_PassesFiltersToRepositoryAndMapsResult()
        {
            var filters = new List<FilterItem> { new(nameof(ShoppingListModel.Name), "List1", FilterOperator.Equals) };
            var entities = new List<Data.Entities.ShoppingList> { new() { Id = Guid.NewGuid(), Name = "List1" } };
            var models = new List<ShoppingListModel> { new() { Name = "List1" } };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", filters, null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.ShoppingList>(entities, 1, 0));
            _mapperMock.Setup(m => m.Map<IList<ShoppingListModel>>(entities)).Returns(models);

            var qp = new QueryParameters<ShoppingListModel> { Filters = filters, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items.Single().Name, Is.EqualTo("List1"));
        }

        [Test]
        public async Task Handle_WithSorting_PassesSortingToRepositoryAndMapsResult()
        {
            var sorting = new List<SortingModel> { new() { PropertyName = nameof(ShoppingListModel.Name), Direction = SortDirection.Ascending } };
            var entities = new List<Data.Entities.ShoppingList>
            {
                new() { Id = Guid.NewGuid(), Name = "List1" },
                new() { Id = Guid.NewGuid(), Name = "List2" }
            };
            var models = new List<ShoppingListModel> { new() { Name = "List1" }, new() { Name = "List2" } };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", null, sorting, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.ShoppingList>(entities, 2, 0));
            _mapperMock.Setup(m => m.Map<IList<ShoppingListModel>>(entities)).Returns(models);

            var qp = new QueryParameters<ShoppingListModel> { Filters = null, Sorting = sorting, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items.Select(x => x.Name), Is.EqualTo(["List1", "List2"]));
        }

        [Test]
        public async Task Handle_MapperReturnsNull_HandledAsEmptyList()
        {
            var entities = new List<Data.Entities.ShoppingList> { new() { Id = Guid.NewGuid(), Name = "List1" } };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", null, null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.ShoppingList>(entities, 1, 0));
            _mapperMock.Setup(m => m.Map<IList<ShoppingListModel>?>(entities)).Returns((IList<ShoppingListModel>?)null);

            var qp = new QueryParameters<ShoppingListModel> { Filters = null, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(1));
            }

            _repoMock.Verify(r => r.SearchByUserAsync("user1", null, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ShoppingListModel>>(entities), Times.Once);
        }

        private void VerifySearchByUserAsyncNeverCalled() =>
            _repoMock.Verify(r => r.SearchByUserAsync(
                It.IsAny<string>(), It.IsAny<IEnumerable<FilterItem>?>(),
                It.IsAny<IEnumerable<SortingModel>?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
    }
}
