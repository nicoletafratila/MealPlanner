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
            _repoMock.Verify(r => r.GetAllByUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _mapperMock.Verify(m => m.Map<IList<RecipeModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NullQueryParameters_ReturnsEmptyPagedList()
        {
            var query = new SearchQuery
            {
                CategoryId = null,
                QueryParameters = null
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.Zero);
            }
            _repoMock.Verify(r => r.GetAllByUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _mapperMock.Verify(m => m.Map<IList<RecipeModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoFiltersOrSorting_MapsAndPaginatesAllResults()
        {
            var cat10 = Guid.NewGuid();
            var cat20 = Guid.NewGuid();

            var entities = new List<Data.Entities.Recipe>
            {
                new() { Id = Guid.NewGuid(), Name = "R1", RecipeCategoryId = cat10 },
                new() { Id = Guid.NewGuid(), Name = "R2", RecipeCategoryId = cat20 }
            };

            var models = new List<RecipeModel>
            {
                new() { Id = Guid.NewGuid(), Name = "R1", RecipeCategoryId = cat10.ToString() },
                new() { Id = Guid.NewGuid(), Name = "R2", RecipeCategoryId = cat20.ToString() }
            };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<RecipeModel>>(entities))
                .Returns(models);

            var qp = new QueryParameters<RecipeModel>
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

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items.Select(x => x.Name), Is.EquivalentTo(["R1", "R2"]));
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(2));
            }

            _repoMock.Verify(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<RecipeModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_CategoryFilter_AppliesFilterBeforePaging()
        {
            var cat10 = Guid.NewGuid();
            var cat20 = Guid.NewGuid();

            var entities = new List<Data.Entities.Recipe>
            {
                new() { Id = Guid.NewGuid(), Name = "R1", RecipeCategoryId = cat10 },
                new() { Id = Guid.NewGuid(), Name = "R2", RecipeCategoryId = cat20 },
                new() { Id = Guid.NewGuid(), Name = "R3", RecipeCategoryId = cat10 },
            };

            var models = new List<RecipeModel>
            {
                new() { Id = Guid.NewGuid(), Name = "R1", RecipeCategoryId = cat10.ToString() },
                new() { Id = Guid.NewGuid(), Name = "R2", RecipeCategoryId = cat20.ToString() },
                new() { Id = Guid.NewGuid(), Name = "R3", RecipeCategoryId = cat10.ToString() },
            };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<RecipeModel>>(entities))
                .Returns(models);

            var qp = new QueryParameters<RecipeModel>
            {
                Filters = null,
                Sorting = null,
                PageNumber = 1,
                PageSize = 10
            };

            var query = new SearchQuery
            {
                CategoryId = cat10.ToString(),
                QueryParameters = qp
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items, Has.Count.EqualTo(2));
            Assert.That(result.Items.Select(x => x.Name), Is.EquivalentTo(["R1", "R3"]));

            _repoMock.Verify(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<RecipeModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_HandledAsEmptyList()
        {
            var entities = new List<Data.Entities.Recipe>
            {
                new() { Id = Guid.NewGuid(), Name = "R1", RecipeCategoryId = Guid.NewGuid() }
            };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<RecipeModel>?>(entities))
                .Returns((IList<RecipeModel>?)null);

            var qp = new QueryParameters<RecipeModel>
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

            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.Zero);
            }

            _repoMock.Verify(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<RecipeModel>>(entities), Times.Once);
        }
    }
}
