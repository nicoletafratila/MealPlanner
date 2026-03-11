using AutoMapper;
using Common.Pagination;
using MealPlanner.Api.Features.MealPlan.Queries.Search;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using Moq;

namespace MealPlanner.Api.Tests.Features.MealPlan.Queries.Search
{
    [TestFixture]
    public class SearchQueryHandlerTests
    {
        private Mock<IMealPlanRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private SearchQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IMealPlanRepository>(MockBehavior.Strict);
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

            Assert.That(result.Items, Is.Empty);
            Assert.That(result.Metadata.TotalCount, Is.EqualTo(0));
            _repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
            _mapperMock.Verify(m => m.Map<IList<MealPlanModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NullQueryParameters_ReturnsEmptyPagedList()
        {
            var query = new SearchQuery { QueryParameters = null };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items, Is.Empty);
            Assert.That(result.Metadata.TotalCount, Is.EqualTo(0));
            _repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
            _mapperMock.Verify(m => m.Map<IList<MealPlanModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoFiltersOrSorting_MapsAndPaginatesAllResults()
        {
            var entities = new List<Common.Data.Entities.MealPlan>
            {
                new() { Id = 1, Name = "Plan1" },
                new() { Id = 2, Name = "Plan2" }
            };

            var models = new List<MealPlanModel>
            {
                new() { Id = 1, Name = "Plan1" },
                new() { Id = 2, Name = "Plan2" }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<MealPlanModel>>(entities))
                .Returns(models);

            var qp = new QueryParameters<MealPlanModel>
            {
                Filters = null,
                Sorting = null,
                PageNumber = 1,
                PageSize = 10
            };

            var query = new SearchQuery { QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items.Count, Is.EqualTo(2));
            Assert.That(result.Items.Select(x => x.Id), Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(result.Metadata.TotalCount, Is.EqualTo(2));

            _repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<MealPlanModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_HandledAsEmptyList()
        {
            var entities = new List<Common.Data.Entities.MealPlan>
            {
                new() { Id = 1, Name = "Plan1" }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<MealPlanModel>?>(entities))
                .Returns((IList<MealPlanModel>?)null);

            var qp = new QueryParameters<MealPlanModel>
            {
                Filters = null,
                Sorting = null,
                PageNumber = 1,
                PageSize = 10
            };

            var query = new SearchQuery { QueryParameters = qp };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Items, Is.Empty);
            Assert.That(result.Metadata.TotalCount, Is.EqualTo(0));

            _repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<MealPlanModel>>(entities), Times.Once);
        }
    }
}