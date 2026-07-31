using Common.Models;
using Common.Pagination;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Mobile.ViewModels.MealPlans;
using Moq;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.MealPlans
{
    [TestFixture]
    public class MealPlansOverviewViewModelTests
    {
        private Mock<IMealPlanService> _mealPlanServiceMock = null!;
        private MealPlansOverviewViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _mealPlanServiceMock = new Mock<IMealPlanService>(MockBehavior.Strict);
            _viewModel = new MealPlansOverviewViewModel(_mealPlanServiceMock.Object);
        }

        [Test]
        public async Task SearchAsync_NoSearchText_PopulatesMealPlansAndHasNextPage()
        {
            var items = new List<MealPlanModel>
            {
                new(Guid.NewGuid(), "Week 1"),
                new(Guid.NewGuid(), "Week 2")
            };
            var metadata = Metadata.Create(1, 20, 40);

            _mealPlanServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<MealPlanModel>>(p => p.Filters == null && p.PageNumber == 1), CancellationToken.None))
                .ReturnsAsync(new PagedList<MealPlanModel>(items, metadata));

            await _viewModel.SearchCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.MealPlans, Has.Count.EqualTo(2));
                Assert.That(_viewModel.HasNextPage, Is.True);
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task SearchAsync_WithSearchText_PassesNameContainsFilter()
        {
            _viewModel.SearchText = "week";
            var metadata = Metadata.Create(1, 20, 1);

            QueryParameters<MealPlanModel>? captured = null;
            _mealPlanServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>(), CancellationToken.None))
                .Callback<QueryParameters<MealPlanModel>, CancellationToken>((p, _) => captured = p)
                .ReturnsAsync(new PagedList<MealPlanModel>([new MealPlanModel(Guid.NewGuid(), "Week 1")], metadata));

            await _viewModel.SearchCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(captured, Is.Not.Null);
                Assert.That(captured!.Filters, Is.Not.Null);
                var filter = captured.Filters!.Single();
                Assert.That(filter.PropertyName, Is.EqualTo("Name"));
                Assert.That(filter.Value, Is.EqualTo("week"));
                Assert.That(filter.Operator, Is.EqualTo(FilterOperator.Contains));
                Assert.That(filter.StringComparison, Is.EqualTo(StringComparison.OrdinalIgnoreCase));
            }
        }

        [Test]
        public async Task SearchAsync_WhenIsBusy_DoesNotCallService()
        {
            _viewModel.IsBusy = true;

            await _viewModel.SearchCommand.ExecuteAsync(null);

            _mealPlanServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task SearchAsync_ServiceThrows_SetsErrorMessage()
        {
            _mealPlanServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>(), CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await _viewModel.SearchCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("boom"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task LoadAsync_DelegatesToSearchAsync()
        {
            var metadata = Metadata.Create(1, 20, 1);
            _mealPlanServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<MealPlanModel>([new MealPlanModel(Guid.NewGuid(), "Week 1")], metadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.MealPlans, Has.Count.EqualTo(1));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _mealPlanServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>(), CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task NextPageAsync_WhenHasNextPage_AppendsItemsAndIncrementsPage()
        {
            var firstMetadata = Metadata.Create(1, 20, 40);
            _mealPlanServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<MealPlanModel>([new MealPlanModel(Guid.NewGuid(), "Week 1")], firstMetadata));

            await _viewModel.SearchCommand.ExecuteAsync(null);

            var secondMetadata = Metadata.Create(2, 20, 40);
            _mealPlanServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<MealPlanModel>>(p => p.PageNumber == 2), CancellationToken.None))
                .ReturnsAsync(new PagedList<MealPlanModel>([new MealPlanModel(Guid.NewGuid(), "Week 2")], secondMetadata));

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.MealPlans, Has.Count.EqualTo(2));
                Assert.That(_viewModel.CurrentPage, Is.EqualTo(2));
                Assert.That(_viewModel.IsLoadingMore, Is.False);
            }
        }

        [Test]
        public async Task NextPageAsync_WhenNoNextPage_DoesNotCallService()
        {
            _viewModel.HasNextPage = false;

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            _mealPlanServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task NextPageAsync_WhenIsBusy_DoesNotCallService()
        {
            _viewModel.HasNextPage = true;
            _viewModel.IsBusy = true;

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            _mealPlanServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task NextPageAsync_WhenIsLoadingMore_DoesNotCallService()
        {
            _viewModel.HasNextPage = true;
            _viewModel.IsLoadingMore = true;

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            _mealPlanServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task DeleteAsync_Success_RemovesMealPlanFromCollection()
        {
            var mealPlan = new MealPlanModel(Guid.NewGuid(), "Week 1");
            _viewModel.MealPlans.Add(mealPlan);

            _mealPlanServiceMock
                .Setup(s => s.DeleteAsync(mealPlan.Id, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.DeleteCommand.ExecuteAsync(mealPlan);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.MealPlans, Does.Not.Contain(mealPlan));
                Assert.That(_viewModel.ErrorMessage, Is.Null);
            }
        }

        [Test]
        public async Task DeleteAsync_Failure_SetsErrorMessageAndKeepsMealPlan()
        {
            var mealPlan = new MealPlanModel(Guid.NewGuid(), "Week 1");
            _viewModel.MealPlans.Add(mealPlan);

            _mealPlanServiceMock
                .Setup(s => s.DeleteAsync(mealPlan.Id, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("cannot delete"));

            await _viewModel.DeleteCommand.ExecuteAsync(mealPlan);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.MealPlans, Contains.Item(mealPlan));
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("cannot delete"));
            }
        }

        [Test]
        public async Task DeleteAsync_ServiceThrows_SetsErrorMessage()
        {
            var mealPlan = new MealPlanModel(Guid.NewGuid(), "Week 1");
            _viewModel.MealPlans.Add(mealPlan);

            _mealPlanServiceMock
                .Setup(s => s.DeleteAsync(mealPlan.Id, CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await _viewModel.DeleteCommand.ExecuteAsync(mealPlan);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.MealPlans, Contains.Item(mealPlan));
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("boom"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task DeleteAsync_WhenIsBusy_DoesNotCallService()
        {
            var mealPlan = new MealPlanModel(Guid.NewGuid(), "Week 1");
            _viewModel.IsBusy = true;

            await _viewModel.DeleteCommand.ExecuteAsync(mealPlan);

            _mealPlanServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
        }
    }
}
