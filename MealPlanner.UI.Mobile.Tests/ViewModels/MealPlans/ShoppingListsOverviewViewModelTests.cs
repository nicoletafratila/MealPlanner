using Common.Models;
using Common.Pagination;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Mobile.ViewModels.MealPlans;
using Moq;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.MealPlans
{
    [TestFixture]
    public class ShoppingListsOverviewViewModelTests
    {
        private Mock<IShoppingListService> _shoppingListServiceMock = null!;
        private ShoppingListsOverviewViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _shoppingListServiceMock = new Mock<IShoppingListService>(MockBehavior.Strict);
            _viewModel = new ShoppingListsOverviewViewModel(_shoppingListServiceMock.Object);
        }

        [Test]
        public async Task LoadAsync_NoSearchText_PopulatesShoppingLists()
        {
            var items = new List<ShoppingListModel>
            {
                new(Guid.NewGuid(), "Week 1 list"),
                new(Guid.NewGuid(), "Week 2 list")
            };
            var metadata = Metadata.Create(1, 20, 2);

            _shoppingListServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<ShoppingListModel>>(p => p.Filters == null), CancellationToken.None))
                .ReturnsAsync(new PagedList<ShoppingListModel>(items, metadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ShoppingLists, Has.Count.EqualTo(2));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task LoadAsync_WithSearchText_PassesNameContainsFilter()
        {
            _viewModel.SearchText = "week 1";
            var metadata = Metadata.Create(1, 20, 1);

            QueryParameters<ShoppingListModel>? captured = null;
            _shoppingListServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShoppingListModel>>(), CancellationToken.None))
                .Callback<QueryParameters<ShoppingListModel>, CancellationToken>((p, _) => captured = p)
                .ReturnsAsync(new PagedList<ShoppingListModel>([new ShoppingListModel(Guid.NewGuid(), "Week 1 list")], metadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(captured, Is.Not.Null);
                Assert.That(captured!.Filters, Is.Not.Null);
                var filter = captured.Filters!.Single();
                Assert.That(filter.PropertyName, Is.EqualTo("Name"));
                Assert.That(filter.Value, Is.EqualTo("week 1"));
                Assert.That(filter.Operator, Is.EqualTo(FilterOperator.Contains));
                Assert.That(filter.StringComparison, Is.EqualTo(StringComparison.OrdinalIgnoreCase));
            }
        }

        [Test]
        public async Task LoadAsync_WhenIsBusy_DoesNotCallService()
        {
            _viewModel.IsBusy = true;

            await _viewModel.LoadCommand.ExecuteAsync(null);

            _shoppingListServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<ShoppingListModel>>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task LoadAsync_ServiceThrows_SetsErrorMessage()
        {
            _shoppingListServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShoppingListModel>>(), CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("boom"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task SearchText_ClearedAfterSearch_ReloadsAllShoppingLists()
        {
            var metadata = Metadata.Create(1, 20, 2);
            _shoppingListServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<ShoppingListModel>>(p => p.Filters == null), CancellationToken.None))
                .ReturnsAsync(new PagedList<ShoppingListModel>(
                    [new ShoppingListModel(Guid.NewGuid(), "Week 1 list"), new ShoppingListModel(Guid.NewGuid(), "Week 2 list")], metadata));

            _viewModel.SearchText = string.Empty;
            if (_viewModel.SearchCommand.ExecutionTask is { } task)
            {
                await task;
            }

            Assert.That(_viewModel.ShoppingLists, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task DeleteAsync_Success_RemovesShoppingListFromCollection()
        {
            var shoppingList = new ShoppingListModel(Guid.NewGuid(), "Week 1 list");
            _viewModel.ShoppingLists.Add(shoppingList);

            _shoppingListServiceMock
                .Setup(s => s.DeleteAsync(shoppingList.Id, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.DeleteCommand.ExecuteAsync(shoppingList);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ShoppingLists, Does.Not.Contain(shoppingList));
                Assert.That(_viewModel.ErrorMessage, Is.Null);
            }
        }

        [Test]
        public async Task DeleteAsync_Failure_SetsErrorMessageAndKeepsShoppingList()
        {
            var shoppingList = new ShoppingListModel(Guid.NewGuid(), "Week 1 list");
            _viewModel.ShoppingLists.Add(shoppingList);

            _shoppingListServiceMock
                .Setup(s => s.DeleteAsync(shoppingList.Id, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("cannot delete"));

            await _viewModel.DeleteCommand.ExecuteAsync(shoppingList);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ShoppingLists, Contains.Item(shoppingList));
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("cannot delete"));
            }
        }

        [Test]
        public async Task DeleteAsync_ServiceThrows_SetsErrorMessage()
        {
            var shoppingList = new ShoppingListModel(Guid.NewGuid(), "Week 1 list");
            _viewModel.ShoppingLists.Add(shoppingList);

            _shoppingListServiceMock
                .Setup(s => s.DeleteAsync(shoppingList.Id, CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await _viewModel.DeleteCommand.ExecuteAsync(shoppingList);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ShoppingLists, Contains.Item(shoppingList));
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("boom"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task DeleteAsync_WhenIsBusy_DoesNotCallService()
        {
            var shoppingList = new ShoppingListModel(Guid.NewGuid(), "Week 1 list");
            _viewModel.IsBusy = true;

            await _viewModel.DeleteCommand.ExecuteAsync(shoppingList);

            _shoppingListServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
        }
    }
}
