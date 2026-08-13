using Common.Constants.Units;
using Common.Models;
using Common.Pagination;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.RecipeBook
{
    [TestFixture]
    public class UnitsOverviewViewModelTests
    {
        private Mock<IUnitService> _unitServiceMock = null!;
        private UnitsOverviewViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _unitServiceMock = new Mock<IUnitService>(MockBehavior.Strict);
            _viewModel = new UnitsOverviewViewModel(_unitServiceMock.Object);
        }

        [Test]
        public async Task LoadAsync_PopulatesUnitsAndPagination()
        {
            var items = new List<UnitModel>
            {
                new(Guid.NewGuid(), "Kilogram", UnitType.Weight),
                new(Guid.NewGuid(), "Liter", UnitType.Liquid)
            };
            var metadata = Metadata.Create(1, 200, 400);

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<UnitModel>(items, metadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Units, Has.Count.EqualTo(2));
                Assert.That(_viewModel.HasNextPage, Is.True);
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task LoadAsync_NoNextPage_SetsHasNextPageFalse()
        {
            var metadata = Metadata.Create(1, 200, 1);

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<UnitModel>([new UnitModel(Guid.NewGuid(), "Kilogram", UnitType.Weight)], metadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            Assert.That(_viewModel.HasNextPage, Is.False);
        }

        [Test]
        public async Task NextPageAsync_WhenHasNextPage_AppendsItemsAndIncrementsPage()
        {
            var firstMetadata = Metadata.Create(1, 200, 400);
            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<UnitModel>([new UnitModel(Guid.NewGuid(), "Kilogram", UnitType.Weight)], firstMetadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            var secondMetadata = Metadata.Create(2, 200, 400);
            _unitServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<UnitModel>>(p => p.PageNumber == 2), CancellationToken.None))
                .ReturnsAsync(new PagedList<UnitModel>([new UnitModel(Guid.NewGuid(), "Liter", UnitType.Liquid)], secondMetadata));

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Units, Has.Count.EqualTo(2));
                Assert.That(_viewModel.CurrentPage, Is.EqualTo(2));
                Assert.That(_viewModel.IsLoadingMore, Is.False);
            }
        }

        [Test]
        public async Task NextPageAsync_WhenNoNextPage_DoesNotCallService()
        {
            _viewModel.HasNextPage = false;

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            _unitServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task SearchText_ClearedAfterSearch_ReloadsAllUnits()
        {
            var metadata = Metadata.Create(1, 200, 2);
            _unitServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<UnitModel>>(p => p.Filters == null), CancellationToken.None))
                .ReturnsAsync(new PagedList<UnitModel>(
                    [new UnitModel(Guid.NewGuid(), "Kilogram", UnitType.Weight), new UnitModel(Guid.NewGuid(), "Liter", UnitType.Liquid)], metadata));

            _viewModel.SearchText = string.Empty;
            if (_viewModel.SearchCommand.ExecutionTask is { } task)
            {
                await task;
            }

            Assert.That(_viewModel.Units, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task DeleteAsync_Success_RemovesUnitFromCollection()
        {
            var unit = new UnitModel(Guid.NewGuid(), "Kilogram", UnitType.Weight);
            _viewModel.Units.Add(unit);

            _unitServiceMock
                .Setup(s => s.DeleteAsync(unit.Id, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.DeleteCommand.ExecuteAsync(unit);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Units, Does.Not.Contain(unit));
                Assert.That(_viewModel.ErrorMessage, Is.Null);
            }
        }

        [Test]
        public async Task DeleteAsync_Failure_SetsErrorMessageAndKeepsUnit()
        {
            var unit = new UnitModel(Guid.NewGuid(), "Kilogram", UnitType.Weight);
            _viewModel.Units.Add(unit);

            _unitServiceMock
                .Setup(s => s.DeleteAsync(unit.Id, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("cannot delete"));

            await _viewModel.DeleteCommand.ExecuteAsync(unit);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Units, Contains.Item(unit));
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("cannot delete"));
            }
        }
    }
}
