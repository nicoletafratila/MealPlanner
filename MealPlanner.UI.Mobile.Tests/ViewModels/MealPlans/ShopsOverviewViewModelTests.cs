using Common.Models;
using Common.Pagination;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Mobile.ViewModels.MealPlans;
using Moq;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.MealPlans
{
    [TestFixture]
    public class ShopsOverviewViewModelTests
    {
        private Mock<IShopService> _shopServiceMock = null!;
        private ShopsOverviewViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _shopServiceMock = new Mock<IShopService>(MockBehavior.Strict);
            _viewModel = new ShopsOverviewViewModel(_shopServiceMock.Object);
        }

        [Test]
        public async Task LoadAsync_NoSearchText_PopulatesShopsAndHasNextPage()
        {
            var items = new List<ShopModel>
            {
                new(Guid.NewGuid(), "Lidl"),
                new(Guid.NewGuid(), "Kaufland")
            };
            var metadata = Metadata.Create(1, 100, 200);

            _shopServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<ShopModel>>(p => p.Filters == null && p.PageNumber == 1), CancellationToken.None))
                .ReturnsAsync(new PagedList<ShopModel>(items, metadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Shops, Has.Count.EqualTo(2));
                Assert.That(_viewModel.HasNextPage, Is.True);
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task LoadAsync_WithSearchText_PassesNameContainsFilter()
        {
            _viewModel.SearchText = "lidl";
            var metadata = Metadata.Create(1, 100, 1);

            QueryParameters<ShopModel>? captured = null;
            _shopServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>(), CancellationToken.None))
                .Callback<QueryParameters<ShopModel>, CancellationToken>((p, _) => captured = p)
                .ReturnsAsync(new PagedList<ShopModel>([new ShopModel(Guid.NewGuid(), "Lidl")], metadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(captured, Is.Not.Null);
                Assert.That(captured!.Filters, Is.Not.Null);
                var filter = captured.Filters!.Single();
                Assert.That(filter.PropertyName, Is.EqualTo("Name"));
                Assert.That(filter.Value, Is.EqualTo("lidl"));
                Assert.That(filter.Operator, Is.EqualTo(FilterOperator.Contains));
                Assert.That(filter.StringComparison, Is.EqualTo(StringComparison.OrdinalIgnoreCase));
            }
        }

        [Test]
        public async Task LoadAsync_WhenIsBusy_DoesNotCallService()
        {
            _viewModel.IsBusy = true;

            await _viewModel.LoadCommand.ExecuteAsync(null);

            _shopServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task LoadAsync_ServiceThrows_SetsErrorMessage()
        {
            _shopServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>(), CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("boom"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task NextPageAsync_WhenHasNextPage_AppendsItemsAndIncrementsPage()
        {
            var firstMetadata = Metadata.Create(1, 100, 200);
            _shopServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ShopModel>([new ShopModel(Guid.NewGuid(), "Lidl")], firstMetadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            var secondMetadata = Metadata.Create(2, 100, 200);
            _shopServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<ShopModel>>(p => p.PageNumber == 2), CancellationToken.None))
                .ReturnsAsync(new PagedList<ShopModel>([new ShopModel(Guid.NewGuid(), "Kaufland")], secondMetadata));

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Shops, Has.Count.EqualTo(2));
                Assert.That(_viewModel.CurrentPage, Is.EqualTo(2));
                Assert.That(_viewModel.IsLoadingMore, Is.False);
            }
        }

        [Test]
        public async Task NextPageAsync_WhenNoNextPage_DoesNotCallService()
        {
            _viewModel.HasNextPage = false;

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            _shopServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task NextPageAsync_WhenIsBusy_DoesNotCallService()
        {
            _viewModel.HasNextPage = true;
            _viewModel.IsBusy = true;

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            _shopServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task NextPageAsync_WhenIsLoadingMore_DoesNotCallService()
        {
            _viewModel.HasNextPage = true;
            _viewModel.IsLoadingMore = true;

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            _shopServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task DeleteAsync_Success_RemovesShopFromCollection()
        {
            var shop = new ShopModel(Guid.NewGuid(), "Lidl");
            _viewModel.Shops.Add(shop);

            _shopServiceMock
                .Setup(s => s.DeleteAsync(shop.Id, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.DeleteCommand.ExecuteAsync(shop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Shops, Does.Not.Contain(shop));
                Assert.That(_viewModel.ErrorMessage, Is.Null);
            }
        }

        [Test]
        public async Task DeleteAsync_Failure_SetsErrorMessageAndKeepsShop()
        {
            var shop = new ShopModel(Guid.NewGuid(), "Lidl");
            _viewModel.Shops.Add(shop);

            _shopServiceMock
                .Setup(s => s.DeleteAsync(shop.Id, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("cannot delete"));

            await _viewModel.DeleteCommand.ExecuteAsync(shop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Shops, Contains.Item(shop));
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("cannot delete"));
            }
        }

        [Test]
        public async Task DeleteAsync_ServiceThrows_SetsErrorMessage()
        {
            var shop = new ShopModel(Guid.NewGuid(), "Lidl");
            _viewModel.Shops.Add(shop);

            _shopServiceMock
                .Setup(s => s.DeleteAsync(shop.Id, CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await _viewModel.DeleteCommand.ExecuteAsync(shop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Shops, Contains.Item(shop));
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("boom"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task DeleteAsync_WhenIsBusy_DoesNotCallService()
        {
            var shop = new ShopModel(Guid.NewGuid(), "Lidl");
            _viewModel.IsBusy = true;

            await _viewModel.DeleteCommand.ExecuteAsync(shop);

            _shopServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
        }
    }
}
