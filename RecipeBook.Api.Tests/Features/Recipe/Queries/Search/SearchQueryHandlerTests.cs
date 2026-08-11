using AutoMapper;
using Common.Pagination;
using Common.Services;
using Moq;
using RecipeBook.Api.Features.Recipe.Queries.Search;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.Recipe.Queries.Search
{
    [TestFixture]
    public class SearchQueryHandlerTests
    {
        private Mock<IRecipeRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<ICurrentUserService> _currentUserMock = null!;
        private SearchQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IRecipeRepository>(MockBehavior.Strict);
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
            _mapperMock.Verify(m => m.Map<IList<RecipeModel>>(It.IsAny<object>()), Times.Never);
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
            _mapperMock.Verify(m => m.Map<IList<RecipeModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoFiltersOrSorting_MapsPageAndSetsIndexes()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var entities = new List<Data.Entities.Recipe>
            {
                new() { Id = id1, Name = "R1", RecipeCategoryId = Guid.NewGuid() },
                new() { Id = id2, Name = "R2", RecipeCategoryId = Guid.NewGuid() }
            };

            var models = new List<RecipeModel>
            {
                new() { Id = id1, Name = "R1" },
                new() { Id = id2, Name = "R2" }
            };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", null, null, null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.Recipe>(entities, 2, 0));
            _mapperMock.Setup(m => m.Map<IList<RecipeModel>>(entities)).Returns(models);

            var qp = new QueryParameters<RecipeModel> { Filters = null, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { CategoryId = null, QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Has.Count.EqualTo(2));
                Assert.That(result.Items.Select(x => x.Index), Is.EqualTo([1, 2]));
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(2));
            }

            _repoMock.Verify(r => r.SearchByUserAsync("user1", null, null, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<RecipeModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_ThumbnailOnlyFilterTrue_ExtractsFlagAndPassesThumbnailOnlyToRepository()
        {
            var entities = new List<Data.Entities.Recipe> { new() { Id = Guid.NewGuid(), Name = "R1", RecipeCategoryId = Guid.NewGuid() } };
            var models = new List<RecipeModel> { new() { Name = "R1" } };

            _repoMock
                .Setup(r => r.SearchByUserAsync(
                    "user1", null, It.Is<IEnumerable<FilterItem>>(f => f == null || !f.Any()), null, 1, 10, It.IsAny<CancellationToken>(), true))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.Recipe>(entities, 1, 0));
            _mapperMock.Setup(m => m.Map<IList<RecipeModel>>(entities)).Returns(models);

            var filters = new List<FilterItem> { new("ThumbnailOnly", true, FilterOperator.Equals) };
            var qp = new QueryParameters<RecipeModel> { Filters = filters, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { CategoryId = null, QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items, Has.Count.EqualTo(1));
            _repoMock.Verify(r => r.SearchByUserAsync(
                "user1", null, It.Is<IEnumerable<FilterItem>>(f => f == null || !f.Any()), null, 1, 10, It.IsAny<CancellationToken>(), true), Times.Once);
        }

        [Test]
        public async Task Handle_ValidCategoryId_PassesParsedGuidToRepository()
        {
            var categoryId = Guid.NewGuid();
            var entities = new List<Data.Entities.Recipe> { new() { Id = Guid.NewGuid(), Name = "R1", RecipeCategoryId = categoryId } };
            var models = new List<RecipeModel> { new() { Name = "R1" } };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", categoryId, null, null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.Recipe>(entities, 1, 0));
            _mapperMock.Setup(m => m.Map<IList<RecipeModel>>(entities)).Returns(models);

            var qp = new QueryParameters<RecipeModel> { Filters = null, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { CategoryId = categoryId.ToString(), QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items, Has.Count.EqualTo(1));
            _repoMock.Verify(r => r.SearchByUserAsync("user1", categoryId, null, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_MalformedCategoryId_IsIgnoredAndPassesNullToRepository()
        {
            var entities = new List<Data.Entities.Recipe> { new() { Id = Guid.NewGuid(), Name = "R1" } };
            var models = new List<RecipeModel> { new() { Name = "R1" } };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", null, null, null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.Recipe>(entities, 1, 0));
            _mapperMock.Setup(m => m.Map<IList<RecipeModel>>(entities)).Returns(models);

            var qp = new QueryParameters<RecipeModel> { Filters = null, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { CategoryId = "not-a-guid", QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items, Has.Count.EqualTo(1));
            _repoMock.Verify(r => r.SearchByUserAsync("user1", null, null, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_WithFilters_PassesFiltersToRepositoryAndMapsResult()
        {
            var filters = new List<FilterItem> { new(nameof(RecipeModel.Name), "R1", FilterOperator.Equals) };
            var entities = new List<Data.Entities.Recipe> { new() { Id = Guid.NewGuid(), Name = "R1" } };
            var models = new List<RecipeModel> { new() { Name = "R1" } };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", null, filters, null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.Recipe>(entities, 1, 0));
            _mapperMock.Setup(m => m.Map<IList<RecipeModel>>(entities)).Returns(models);

            var qp = new QueryParameters<RecipeModel> { Filters = filters, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { CategoryId = null, QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items.Single().Name, Is.EqualTo("R1"));
        }

        [Test]
        public async Task Handle_WithSorting_PassesSortingToRepositoryAndMapsResult()
        {
            var sorting = new List<SortingModel> { new() { PropertyName = nameof(RecipeModel.Name), Direction = SortDirection.Ascending } };
            var entities = new List<Data.Entities.Recipe>
            {
                new() { Id = Guid.NewGuid(), Name = "R1" },
                new() { Id = Guid.NewGuid(), Name = "R2" }
            };
            var models = new List<RecipeModel> { new() { Name = "R1" }, new() { Name = "R2" } };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", null, null, sorting, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.Recipe>(entities, 2, 0));
            _mapperMock.Setup(m => m.Map<IList<RecipeModel>>(entities)).Returns(models);

            var qp = new QueryParameters<RecipeModel> { Filters = null, Sorting = sorting, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { CategoryId = null, QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items.Select(x => x.Name), Is.EqualTo(["R1", "R2"]));
        }

        [Test]
        public async Task Handle_MapperReturnsNull_HandledAsEmptyList()
        {
            var entities = new List<Data.Entities.Recipe> { new() { Id = Guid.NewGuid(), Name = "R1" } };

            _repoMock
                .Setup(r => r.SearchByUserAsync("user1", null, null, null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.Recipe>(entities, 1, 0));
            _mapperMock.Setup(m => m.Map<IList<RecipeModel>?>(entities)).Returns((IList<RecipeModel>?)null);

            var qp = new QueryParameters<RecipeModel> { Filters = null, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { CategoryId = null, QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(1));
            }

            _repoMock.Verify(r => r.SearchByUserAsync("user1", null, null, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<RecipeModel>>(entities), Times.Once);
        }

        private void VerifySearchByUserAsyncNeverCalled() =>
            _repoMock.Verify(r => r.SearchByUserAsync(
                It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<IEnumerable<FilterItem>?>(),
                It.IsAny<IEnumerable<SortingModel>?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
    }
}
