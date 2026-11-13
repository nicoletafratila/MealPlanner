using AutoMapper;
using Common.Pagination;
using Moq;
using RecipeBook.Api.Features.Job.Queries.Search;
using RecipeBook.Api.Repositories;
using RecipeBook.Data.Entities.Enums;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.Job.Queries.Search
{
    [TestFixture]
    public class SearchQueryHandlerTests
    {
        private Mock<IJobRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private SearchQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IJobRepository>(MockBehavior.Strict);
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
            var result = await _handler.Handle(null!, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.Zero);
            }
            VerifySearchAsyncNeverCalled();
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
            VerifySearchAsyncNeverCalled();
        }

        [Test]
        public async Task Handle_MapsPageAndSetsIndexes()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var entities = new List<Data.Entities.Job>
            {
                new() { Id = id1, Status = JobStatus.Completed },
                new() { Id = id2, Status = JobStatus.Failed }
            };

            var models = new List<JobModel>
            {
                new() { Id = id1 },
                new() { Id = id2 }
            };

            _repoMock
                .Setup(r => r.SearchAsync(null, null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.Job>(entities, 2, 0));
            _mapperMock.Setup(m => m.Map<IList<JobModel>>(entities)).Returns(models);

            var qp = new QueryParameters<JobModel> { Filters = null, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Has.Count.EqualTo(2));
                Assert.That(result.Items.Select(x => x.Index), Is.EqualTo([1, 2]));
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(2));
            }

            _repoMock.Verify(r => r.SearchAsync(null, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_HandledAsEmptyList()
        {
            var entities = new List<Data.Entities.Job> { new() { Id = Guid.NewGuid() } };

            _repoMock
                .Setup(r => r.SearchAsync(null, null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedQueryResult<Data.Entities.Job>(entities, 1, 0));
            _mapperMock.Setup(m => m.Map<IList<JobModel>?>(entities)).Returns((IList<JobModel>?)null);

            var qp = new QueryParameters<JobModel> { Filters = null, Sorting = null, PageNumber = 1, PageSize = 10 };
            var query = new SearchQuery { QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(1));
            }
        }

        private void VerifySearchAsyncNeverCalled() =>
            _repoMock.Verify(r => r.SearchAsync(
                It.IsAny<IEnumerable<FilterItem>?>(), It.IsAny<IEnumerable<SortingModel>?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
    }
}
