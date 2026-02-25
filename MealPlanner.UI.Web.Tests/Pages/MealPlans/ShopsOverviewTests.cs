using System.Reflection;
using BlazorBootstrap;
using Blazored.SessionStorage;
using Bunit;
using Common.Models;
using Common.Pagination;
using Common.UI;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Pages.MealPlans;
using MealPlanner.UI.Web.Services.MealPlans;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MealPlanner.UI.Web.Tests.Pages.MealPlans
{
    [TestFixture]
    public class ShopsOverviewTests
    {
        private BunitContext _ctx = null!;
        private Mock<IShopService> _shopServiceMock = null!;
        private Mock<ISessionStorageService> _sessionStorageMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _shopServiceMock = new Mock<IShopService>(MockBehavior.Strict);
            _sessionStorageMock = new Mock<ISessionStorageService>(MockBehavior.Loose);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _shopServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>()))
                .ReturnsAsync(new PagedList<ShopModel>([], new Metadata()));

            _ctx.Services.AddSingleton(_shopServiceMock.Object);
            _ctx.Services.AddSingleton(_sessionStorageMock.Object);
            _ctx.Services.AddSingleton(_messageComponentMock.Object);

            _ctx.Services.AddBlazorBootstrap();
            _ctx.Services.AddLogging();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
        }

        private IRenderedComponent<ShopsOverview> RenderComponent()
        {
            return _ctx.Render<ShopsOverview>(ps =>
            {
                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
            });
        }

        // ---------- OnInitialized ----------
        [Test]
        public void OnInitialized_SetsBreadcrumb()
        {
            // Act
            var navManager = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(navManager, Is.Not.Null);
        }

        // ---------- New / Update ----------
        [Test]
        public void New_NavigatesToShopEdit()
        {
            // Arrange
            var cut = RenderComponent();
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            var method = typeof(ShopsOverview).GetMethod("New", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            cut.InvokeAsync(() => method!.Invoke(cut.Instance, []));

            // Assert
            Assert.That(nav.Uri, Does.EndWith("mealplans/shopedit/"));
        }

        [Test]
        public void Update_NavigatesToShopEdit_WithId()
        {
            // Arrange
            var cut = RenderComponent();
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            var method = typeof(ShopsOverview).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var shop = new ShopModel { Id = 7 };

            // Act
            cut.InvokeAsync(() => method!.Invoke(cut.Instance, [shop]));

            // Assert
            Assert.That(nav.Uri, Does.EndWith("mealplans/shopedit/7"));
        }

        // ---------- DeleteAsync / DeleteCoreAsync ----------
        [Test]
        public async Task DeleteAsync_DoesNothing_WhenItemIsNull()
        {
            // Arrange
            var cut = RenderComponent();

            var method = typeof(ShopsOverview).GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [null!])!;
                await task;
            });

            // Assert
            _shopServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public async Task DeleteCoreAsync_DeletesAndRefreshes_WhenSucceeded()
        {
            // Arrange
            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            _shopServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync(response);

            var cut = RenderComponent();

            var method = typeof(ShopsOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var item = new ShopModel { Id = 5, Name = "Shop" };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [item])!;
                await task;
            });

            // Assert
            _shopServiceMock.Verify(s => s.DeleteAsync(5), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been deleted successfully"),
                Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            _shopServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent();

            var method = typeof(ShopsOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var item = new ShopModel { Id = 5 };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [item])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowError("Delete failed. Please try again."),
                Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsResponseMessage_WhenFailed()
        {
            // Arrange
            var response = new CommandResponse
            {
                Succeeded = false,
                Message = "Delete failed because of dependency"
            };

            _shopServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync(response);

            var cut = RenderComponent();

            var method = typeof(ShopsOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var item = new ShopModel { Id = 5 };

            // Act
            await cut.InvokeAsync(async () =>
            {
                Task task = (Task)method!.Invoke(cut.Instance, [item])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowError("Delete failed because of dependency"),
                Times.Once);
        }

        // ---------- DataProviderAsync ----------
        [Test]
        public async Task DataProviderAsync_ReturnsEmpty_WhenServiceReturnsNull()
        {
            // Arrange
            _shopServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>()))
                .ReturnsAsync((PagedList<ShopModel>?)null);

            var cut = RenderComponent();

            var method = typeof(ShopsOverview).GetMethod("DataProviderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var request = new GridDataProviderRequest<ShopModel>
            {
                Filters = [],
                Sorting = [],
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            GridDataProviderResult<ShopModel> result = default!;
            await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<ShopModel>>)method!.Invoke(cut.Instance, [request])!;
                result = await task;
            });

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.Data, Is.Empty);
                Assert.That(result.TotalCount, Is.EqualTo(0));
            });
        }

        [Test]
        public async Task DataProviderAsync_SetsCssClassAndTotalCount_WhenItemsReturned()
        {
            // Arrange
            var items = new List<ShopModel>
            {
                new() { Id = 1, Name = "Shop1" }
            };

            var paged = new PagedList<ShopModel>(
                items,
                new Metadata { TotalCount = 1, PageNumber = 1, PageSize = 10 });

            _shopServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>()))
                .ReturnsAsync(paged);

            var cut = RenderComponent();

            var method = typeof(ShopsOverview).GetMethod("DataProviderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var request = new GridDataProviderRequest<ShopModel>
            {
                Filters = [],
                Sorting = [],
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            GridDataProviderResult<ShopModel> result = default!;
            await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<ShopModel>>)method!.Invoke(cut.Instance, [request])!;
                result = await task;
            });

            // Assert
            Assert.That(result!.Data!.Count, Is.EqualTo(1));
            Assert.That(result.TotalCount, Is.EqualTo(1));
        }
    }
}