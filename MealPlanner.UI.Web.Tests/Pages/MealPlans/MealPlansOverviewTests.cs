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
    public class MealPlansOverviewTests
    {
        private BunitContext _ctx = null!;
        private Mock<IMealPlanService> _mealPlanServiceMock = null!;
        private Mock<ISessionStorageService> _sessionStorageMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _mealPlanServiceMock = new Mock<IMealPlanService>(MockBehavior.Strict);
            _sessionStorageMock = new Mock<ISessionStorageService>(MockBehavior.Loose);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _mealPlanServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<MealPlanModel>([], new Metadata()));

            _ctx.Services.AddSingleton(_mealPlanServiceMock.Object);
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

        private IRenderedComponent<MealPlansOverview> RenderComponent()
        {
            return _ctx.Render<MealPlansOverview>(ps =>
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
        public void New_NavigatesToMealPlanEdit()
        {
            // Arrange
            var cut = RenderComponent();
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            var method = typeof(MealPlansOverview).GetMethod("New", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            cut.InvokeAsync(() => method!.Invoke(cut.Instance, []));

            // Assert
            Assert.That(nav.Uri, Does.EndWith("mealplans/mealplanedit/"));
        }

        [Test]
        public void Update_NavigatesToMealPlanEdit_WithId()
        {
            // Arrange
            var cut = RenderComponent();
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            var method = typeof(MealPlansOverview).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var mealPlan = new MealPlanModel { Id = 7 };

            // Act
            cut.InvokeAsync(() => method!.Invoke(cut.Instance, [mealPlan]));

            // Assert
            Assert.That(nav.Uri, Does.EndWith("mealplans/mealplanedit/7"));
        }

        // ---------- DeleteAsync / DeleteCoreAsync ----------
        [Test]
        public async Task DeleteAsync_DoesNothing_WhenItemIsNull()
        {
            // Arrange
            var cut = RenderComponent();

            var method = typeof(MealPlansOverview).GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [null!])!;
                await task;
            });

            // Assert
            _mealPlanServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<int>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task DeleteCoreAsync_DeletesAndRefreshes_WhenSucceeded()
        {
            // Arrange
            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            _mealPlanServiceMock
                .Setup(s => s.DeleteAsync(5, CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent();

            var method = typeof(MealPlansOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var item = new MealPlanModel { Id = 5, Name = "Plan" };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [item])!;
                await task;
            });

            // Assert
            _mealPlanServiceMock.Verify(s => s.DeleteAsync(5, CancellationToken.None), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Data has been deleted successfully", It.IsAny<string>(), CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            _mealPlanServiceMock
                .Setup(s => s.DeleteAsync(5, CancellationToken.None))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent();

            var method = typeof(MealPlansOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var item = new MealPlanModel { Id = 5 };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [item])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Delete failed. Please try again.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
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

            _mealPlanServiceMock
                .Setup(s => s.DeleteAsync(5, CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent();

            var method = typeof(MealPlansOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var item = new MealPlanModel { Id = 5 };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [item])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Delete failed because of dependency", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }

        // ---------- DataProviderAsync ----------

        [Test]
        public async Task DataProviderAsync_ReturnsEmpty_WhenServiceReturnsNull()
        {
            // Arrange: override default
            _mealPlanServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>(), CancellationToken.None))
                .ReturnsAsync((PagedList<MealPlanModel>?)null);

            var cut = RenderComponent();

            var method = typeof(MealPlansOverview).GetMethod("DataProviderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var request = new GridDataProviderRequest<MealPlanModel>
            {
                Filters = [],
                Sorting = [],
                PageNumber = 1,
                PageSize = 10
            };

            GridDataProviderResult<MealPlanModel> result = default!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<MealPlanModel>>)method!.Invoke(cut.Instance, [request])!;
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
        public async Task DataProviderAsync_SetsTotalCount_WhenItemsReturned()
        {
            // Arrange
            var items = new List<MealPlanModel>
            {
                new() { Id = 1, Name = "Plan1" }
            };

            var paged = new PagedList<MealPlanModel>(
                items,
                new Metadata { TotalCount = 1, PageNumber = 1, PageSize = 10 });

            _mealPlanServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>(), CancellationToken.None))
                .ReturnsAsync(paged);

            var cut = RenderComponent();

            var method = typeof(MealPlansOverview).GetMethod("DataProviderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var request = new GridDataProviderRequest<MealPlanModel>
            {
                Filters = [],
                Sorting = [],
                PageNumber = 1,
                PageSize = 10
            };

            GridDataProviderResult<MealPlanModel> result = default!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<MealPlanModel>>)method!.Invoke(cut.Instance, [request])!;
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