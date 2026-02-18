using System.Reflection;
using BlazorBootstrap;
using Blazored.SessionStorage;
using Bunit;
using Common.Models;
using Common.Pagination;
using Common.UI;
using MealPlanner.UI.Web.Pages.RecipeBooks;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Tests.Pages.RecipeBooks
{
    [TestFixture]
    public class UnitsOverviewTests
    {
        private const string RecipeBooks_UnitEdit = "recipebooks/unitedit/";

        private BunitContext _ctx = null!;
        private Mock<IUnitService> _unitServiceMock = null!;
        private Mock<ISessionStorageService> _sessionStorageMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _unitServiceMock = new Mock<IUnitService>(MockBehavior.Strict);
            _sessionStorageMock = new Mock<ISessionStorageService>(MockBehavior.Strict);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_unitServiceMock.Object);
            _ctx.Services.AddSingleton(_sessionStorageMock.Object);

            _ctx.Services.AddScoped<BreadcrumbService>();
            _ctx.Services.AddScoped<ModalService>();
            _ctx.Services.AddScoped<PreloadService>();

            _unitServiceMock
               .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>()))
               .ReturnsAsync(new PagedList<UnitModel>(new List<UnitModel>(), new Metadata()));

            _sessionStorageMock
                .Setup(s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            _ctx.JSInterop.SetupVoid("window.blazorBootstrap.dropdown.initialize", _ => true);
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
            _unitServiceMock.Reset();
            _sessionStorageMock.Reset();
            _messageComponentMock.Reset();
        }

        private IRenderedComponent<UnitsOverview> RenderWithMessageComponent()
        {
            return _ctx.Render<UnitsOverview>(parameters =>
            {
                parameters.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
            });
        }

        // ---------- Navigation ----------
        [Test]
        public void New_NavigatesToCreatePage()
        {
            // Arrange
            var navManager = _ctx.Services.GetRequiredService<NavigationManager>();
            var cut = RenderWithMessageComponent();

            // Act
            cut.InvokeAsync(() =>
            {
                var m = typeof(UnitsOverview).GetMethod("New", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(m, Is.Not.Null);
                m!.Invoke(cut.Instance, []);
            });

            // Assert
            Assert.That(navManager.Uri, Does.EndWith(RecipeBooks_UnitEdit));
        }

        [Test]
        public void Update_NavigatesToEditPage_ForGivenItem()
        {
            // Arrange
            var navManager = (NavigationManager)_ctx.Services.GetRequiredService<NavigationManager>();
            var cut = RenderWithMessageComponent();

            var method = typeof(UnitsOverview).GetMethod("Update",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var model = new UnitModel { Id = 42 };

            // Act
            cut.InvokeAsync(() => method!.Invoke(cut.Instance, [model]));

            // Assert
            Assert.That(navManager.Uri, Does.EndWith($"{RecipeBooks_UnitEdit}42"));
        }

        // ---------- DeleteAsync ----------
        [Test]
        public async Task DeleteCoreAsync_WhenDeleteSucceeds_ShowsInfo_AndRefreshesGrid()
        {
            // Arrange
            var unit = new UnitModel { Id = 5 };

            _unitServiceMock
                .Setup(s => s.DeleteAsync(unit.Id))
                .ReturnsAsync(new CommandResponse { Succeeded = true, Message = "ok" });

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>()))
                .ReturnsAsync(new PagedList<UnitModel>(new List<UnitModel>(), new Metadata()));

            _sessionStorageMock
                .Setup(s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            var cut = RenderWithMessageComponent();

            var method = typeof(UnitsOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "DeleteCoreAsync method not found via reflection.");

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { unit })!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been deleted successfully"),
                Times.Once);

            _unitServiceMock.Verify(s => s.DeleteAsync(unit.Id), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_WhenDeleteFails_ShowsError()
        {
            // Arrange
            var unit = new UnitModel { Id = 7 };
            var response = new CommandResponse { Succeeded = false, Message = "delete failed" };

            _unitServiceMock
                .Setup(s => s.DeleteAsync(unit.Id))
                .ReturnsAsync(response);

            var cut = RenderWithMessageComponent();

            var method = typeof(UnitsOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [unit])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(m => m.ShowError("delete failed"), Times.Once);
        }

        // ---------- DataProviderAsync ----------
        [Test]
        public async Task DataProviderAsync_CallsService_SavesQuery_InSessionStorage_AndReturnsData()
        {
            // Arrange
            var items = new List<UnitModel>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };

            var metadata = new Metadata
            {
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 2
            };

            var paged = new PagedList<UnitModel>(items, metadata);

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>()))
                .ReturnsAsync(paged);

            _sessionStorageMock
                .Setup(s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            var cut = RenderWithMessageComponent();

            var method = typeof(UnitsOverview).GetMethod("DataProviderAsync",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var request = new GridDataProviderRequest<UnitModel>
            {
                Filters = null,
                Sorting = null,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            GridDataProviderResult<UnitModel> result = await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<UnitModel>>)method!.Invoke(cut.Instance, [request])!;
                return await task;
            });

            _unitServiceMock.Verify(
                s => s.SearchAsync(It.Is<QueryParameters<UnitModel>>(q =>
                    q.PageNumber == 1 && q.PageSize == 10)),
                Times.Exactly(2));

            _sessionStorageMock.Verify(
                s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));

            Assert.Multiple(() =>
            {
                Assert.That(result.Data!.Count, Is.EqualTo(2));
                Assert.That(result.TotalCount, Is.EqualTo(2));
            });
        }
    }
}