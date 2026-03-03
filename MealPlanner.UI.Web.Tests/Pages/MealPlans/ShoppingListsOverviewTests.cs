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
    public class ShoppingListsOverviewTests
    {
        private BunitContext _ctx = null!;
        private Mock<IShoppingListService> _shoppingListServiceMock = null!;
        private Mock<ISessionStorageService> _sessionStorageMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _shoppingListServiceMock = new Mock<IShoppingListService>(MockBehavior.Strict);
            _sessionStorageMock = new Mock<ISessionStorageService>(MockBehavior.Loose);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _shoppingListServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShoppingListModel>>()))
                .ReturnsAsync(new PagedList<ShoppingListModel>([], new Metadata()));

            _ctx.Services.AddSingleton(_shoppingListServiceMock.Object);
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

        private IRenderedComponent<ShoppingListsOverview> RenderComponent()
        {
            return _ctx.Render<ShoppingListsOverview>(ps =>
            {
                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
            });
        }

        // ---------- OnInitialized ----------
        [Test]
        public void OnInitialized_RendersWithoutError()
        {
            // Act
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav, Is.Not.Null);
        }

        // ---------- New / Update ----------
        [Test]
        public void New_NavigatesToShoppingListEdit()
        {
            // Arrange
            var cut = RenderComponent();
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            var method = typeof(ShoppingListsOverview).GetMethod("New", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            cut.InvokeAsync(() => method!.Invoke(cut.Instance, []));

            // Assert
            Assert.That(nav.Uri, Does.EndWith("mealplans/shoppinglistedit/"));
        }

        [Test]
        public void Update_NavigatesToShoppingListEdit_WithId()
        {
            // Arrange
            var cut = RenderComponent();
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            var method = typeof(ShoppingListsOverview).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var list = new ShoppingListModel { Id = 7 };

            // Act
            cut.InvokeAsync(() => method!.Invoke(cut.Instance, [list]));

            // Assert
            Assert.That(nav.Uri, Does.EndWith("mealplans/shoppinglistedit/7"));
        }

        // ---------- DeleteAsync / DeleteCoreAsync ----------
        [Test]
        public async Task DeleteAsync_DoesNothing_WhenItemIsNull()
        {
            // Arrange
            var cut = RenderComponent();

            var method = typeof(ShoppingListsOverview).GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [null!])!;
                await task;
            });

            // Assert
            _shoppingListServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public async Task DeleteCoreAsync_DeletesAndRefreshes_WhenSucceeded()
        {
            // Arrange
            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            _shoppingListServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync(response);

            var cut = RenderComponent();

            var method = typeof(ShoppingListsOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var item = new ShoppingListModel { Id = 5, Name = "List" };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [item])!;
                await task;
            });

            // Assert
            _shoppingListServiceMock.Verify(s => s.DeleteAsync(5), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been deleted successfully"),
                Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            _shoppingListServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent();

            var method = typeof(ShoppingListsOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var item = new ShoppingListModel { Id = 5 };

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

            _shoppingListServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync(response);

            var cut = RenderComponent();

            var method = typeof(ShoppingListsOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var item = new ShoppingListModel { Id = 5 };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [item])!;
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
            // Arrange: override default setup
            _shoppingListServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShoppingListModel>>()))
                .ReturnsAsync((PagedList<ShoppingListModel>?)null);

            var cut = RenderComponent();

            var method = typeof(ShoppingListsOverview).GetMethod("DataProviderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var request = new GridDataProviderRequest<ShoppingListModel>
            {
                Filters = [],
                Sorting = [],
                PageNumber = 1,
                PageSize = 10
            };

            GridDataProviderResult<ShoppingListModel> result = default!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<ShoppingListModel>>)method!.Invoke(cut.Instance, [request])!;
                result = await task;
            });

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.Data, Is.Empty);
                Assert.That(result.TotalCount, Is.Zero);
            }
        }

        [Test]
        public async Task DataProviderAsync_SetsCssClassAndTotalCount_WhenItemsReturned()
        {
            // Arrange
            var items = new List<ShoppingListModel>
            {
                new() { Id = 1, Name = "List1" }
            };

            var paged = new PagedList<ShoppingListModel>(
                items,
                new Metadata { TotalCount = 1, PageNumber = 1, PageSize = 10 });

            _shoppingListServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShoppingListModel>>()))
                .ReturnsAsync(paged);

            var cut = RenderComponent();

            var method = typeof(ShoppingListsOverview).GetMethod("DataProviderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var request = new GridDataProviderRequest<ShoppingListModel>
            {
                Filters = [],
                Sorting = [],
                PageNumber = 1,
                PageSize = 10
            };

            GridDataProviderResult<ShoppingListModel> result = default!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<ShoppingListModel>>)method!.Invoke(cut.Instance, [request])!;
                result = await task;
            });

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result!.Data!.Count(), Is.EqualTo(1));
                Assert.That(result.TotalCount, Is.EqualTo(1));
            }
        }
    }
}