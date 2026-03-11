using AutoMapper;
using MealPlanner.Api.Features.MealPlan.Queries.SearchByRecipeId;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using Moq;

namespace MealPlanner.Api.Tests.Features.MealPlan.Queries.SearchByRecipeId
{
    [TestFixture]
    public class SearchByRecipeIdQueryHandlerTests
    {
        private Mock<IMealPlanRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private SearchByRecipeIdQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IMealPlanRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _handler = new SearchByRecipeIdQueryHandler(_repoMock.Object, _mapperMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new SearchByRecipeIdQueryHandler(null!, _mapperMock.Object));
        }

        [Test]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new SearchByRecipeIdQueryHandler(_repoMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_ReturnsMappedMealPlans_WithIndexesSet()
        {
            // Arrange
            const int recipeId = 10;
            var query = new SearchByRecipeIdQuery(recipeId);

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
                .Setup(r => r.SearchByRecipeAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<MealPlanModel>>(entities))
                .Returns(models);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Index, Is.EqualTo(1));
            Assert.That(result[1].Index, Is.EqualTo(2));

            _repoMock.Verify(r => r.SearchByRecipeAsync(recipeId, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<MealPlanModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_HandledAsEmptyList()
        {
            // Arrange
            const int recipeId = 10;
            var query = new SearchByRecipeIdQuery(recipeId);

            var entities = new List<Common.Data.Entities.MealPlan>
            {
                new() { Id = 1, Name = "Plan1" }
            };

            _repoMock
                .Setup(r => r.SearchByRecipeAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<MealPlanModel>?>(entities))
                .Returns((IList<MealPlanModel>?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);

            _repoMock.Verify(r => r.SearchByRecipeAsync(recipeId, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<MealPlanModel>>(entities), Times.Once);
        }
    }
}