using Common.Models;
using Common.Pagination;
using Identity.Services.Http;
using Identity.Shared.Models;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Mobile.Services;
using MealPlanner.UI.Mobile.ViewModels.MealPlans;
using Moq;
using RecipeBook.Services.Http;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.MealPlans
{
    [TestFixture]
    public class ShopsOverviewViewModelTests
    {
        private Mock<IShopService> _shopServiceMock = null!;
        private Mock<IApplicationUserService> _applicationUserServiceMock = null!;
        private ShopsOverviewViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _shopServiceMock = new Mock<IShopService>(MockBehavior.Strict);
            _applicationUserServiceMock = new Mock<IApplicationUserService>(MockBehavior.Strict);

            var lookupDataService = new ReferenceDataCacheService(
                Mock.Of<IRecipeCategoryService>(),
                Mock.Of<IUnitService>(),
                Mock.Of<IProductService>(),
                Mock.Of<IProductCategoryService>(),
                _shopServiceMock.Object,
                Mock.Of<IRecipeService>());

            _viewModel = new ShopsOverviewViewModel(_shopServiceMock.Object, _applicationUserServiceMock.Object, lookupDataService);
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
        public async Task SearchText_ClearedAfterSearch_ReloadsAllShops()
        {
            var metadata = Metadata.Create(1, 100, 2);
            _shopServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<ShopModel>>(p => p.Filters == null), CancellationToken.None))
                .ReturnsAsync(new PagedList<ShopModel>(
                    [new ShopModel(Guid.NewGuid(), "Lidl"), new ShopModel(Guid.NewGuid(), "Kaufland")], metadata));

            _viewModel.SearchText = string.Empty;
            if (_viewModel.SearchCommand.ExecutionTask is { } task)
            {
                await task;
            }

            Assert.That(_viewModel.Shops, Has.Count.EqualTo(2));
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

        // ---------- GetShareUsersAsync ----------
        [Test]
        public async Task GetShareUsersAsync_ReturnsUsersFromService()
        {
            var users = new List<ApplicationUserListModel> { new() { UserId = "2", Username = "bob" } };

            _applicationUserServiceMock
                .Setup(s => s.ListAsync(CancellationToken.None))
                .ReturnsAsync(users);

            var result = await _viewModel.GetShareUsersAsync();

            Assert.That(result, Is.EquivalentTo(users));
        }

        [Test]
        public async Task GetShareUsersAsync_NullFromService_ReturnsEmptyList()
        {
            _applicationUserServiceMock
                .Setup(s => s.ListAsync(CancellationToken.None))
                .ReturnsAsync((IList<ApplicationUserListModel>?)null);

            var result = await _viewModel.GetShareUsersAsync();

            Assert.That(result, Is.Empty);
        }

        // ---------- ShareAllToUserAsync ----------
        [Test]
        public async Task ShareAllToUserAsync_EmptyTargetUserId_DoesNotCallService()
        {
            await _viewModel.ShareAllToUserAsync(string.Empty);

            _shopServiceMock.Verify(s => s.ShareAllAsync(It.IsAny<string>(), It.IsAny<IEnumerable<FilterItem>?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ShareAllToUserAsync_WhenBusy_DoesNotCallService()
        {
            _viewModel.IsBusy = true;

            await _viewModel.ShareAllToUserAsync("user2");

            _shopServiceMock.Verify(s => s.ShareAllAsync(It.IsAny<string>(), It.IsAny<IEnumerable<FilterItem>?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ShareAllToUserAsync_ServiceSucceeds_SetsSuccessMessage_AndClearsBusy()
        {
            _shopServiceMock
                .Setup(s => s.ShareAllAsync("user2", null, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.ShareAllToUserAsync("user2");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.SuccessMessage, Is.Not.Null.And.Not.Empty);
                Assert.That(_viewModel.ErrorMessage, Is.Null);
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task ShareAllToUserAsync_WithActiveSearchFilter_PassesFilterToService()
        {
            _viewModel.SearchText = "Lidl";

            IEnumerable<FilterItem>? capturedFilters = null;
            _shopServiceMock
                .Setup(s => s.ShareAllAsync("user2", It.IsAny<IEnumerable<FilterItem>?>(), CancellationToken.None))
                .Callback<string, IEnumerable<FilterItem>?, CancellationToken>((_, filters, _) => capturedFilters = filters)
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.ShareAllToUserAsync("user2");

            Assert.That(capturedFilters, Is.Not.Null);
            var filter = capturedFilters!.Single();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(filter.PropertyName, Is.EqualTo("Name"));
                Assert.That(filter.Value, Is.EqualTo("Lidl"));
            }
        }

        [Test]
        public async Task ShareAllToUserAsync_ServiceReturnsFailure_SetsErrorFromResponseMessage()
        {
            _shopServiceMock
                .Setup(s => s.ShareAllAsync("user2", null, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("share rejected"));

            await _viewModel.ShareAllToUserAsync("user2");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("share rejected"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }
    }
}
