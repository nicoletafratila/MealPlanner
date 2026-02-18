using System.Reflection;
using BlazorBootstrap;
using Blazored.SessionStorage;
using Bunit;
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
    public class ProductCategoriesOverviewTests
    {
        private const string EditBaseUrl = "recipebooks/productcategoryedit/";

        private BunitContext _ctx = null!;
        private Mock<IProductCategoryService> _serviceMock = null!;
        private Mock<ISessionStorageService> _sessionStorageMock = null!;
        private Mock<IMessageComponent> _messageMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _serviceMock = new Mock<IProductCategoryService>(MockBehavior.Strict);
            _sessionStorageMock = new Mock<ISessionStorageService>(MockBehavior.Strict);
            _messageMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_serviceMock.Object);
            _ctx.Services.AddSingleton(_sessionStorageMock.Object);

            _ctx.Services.AddScoped<BreadcrumbService>();
            _ctx.Services.AddScoped<ModalService>();
            _ctx.Services.AddScoped<PreloadService>();

            _ctx.JSInterop.SetupVoid("window.blazorBootstrap.dropdown.initialize", _ => true);
            _ctx.JSInterop.SetupVoid("window.blazorBootstrap.confirmDialog.show", _ => true);

            _serviceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], new Metadata()));

            _sessionStorageMock
                .Setup(s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
            _serviceMock.Reset();
            _sessionStorageMock.Reset();
            _messageMock.Reset();
        }

        private IRenderedComponent<ProductCategoriesOverview> RenderWithMessageComponent()
        {
            return _ctx.Render<ProductCategoriesOverview>(parameters =>
            {
                parameters.AddCascadingValue("MessageComponent", _messageMock.Object);
            });
        }

        // ---------- Navigation ----------
        [Test]
        public void New_NavigatesToCreatePage()
        {
            var navManager = _ctx.Services.GetRequiredService<NavigationManager>();
            var cut = RenderWithMessageComponent();

            cut.InvokeAsync(() =>
            {
                var m = typeof(ProductCategoriesOverview).GetMethod( "New", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(m, Is.Not.Null);
                m!.Invoke(cut.Instance, []);
            });

            Assert.That(navManager.Uri, Does.EndWith(EditBaseUrl));
        }

        [Test]
        public void Update_NavigatesToEditPage_ForGivenItem()
        {
            var navManager = _ctx.Services.GetRequiredService<NavigationManager>();
            var cut = RenderWithMessageComponent();

            var method = typeof(ProductCategoriesOverview).GetMethod( "Update", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var model = new ProductCategoryModel { Id = 42 };

            cut.InvokeAsync(() => method!.Invoke(cut.Instance, [model]));

            Assert.That(navManager.Uri, Does.EndWith($"{EditBaseUrl}42"));
        }

        // ---------- DeleteCoreAsync ----------
        [Test]
        public async Task DeleteCoreAsync_WhenDeleteSucceeds_ShowsInfo_AndRefreshesGrid()
        {
            var category = new ProductCategoryModel { Id = 5 };

            _serviceMock
                .Setup(s => s.DeleteAsync(category.Id))
                .ReturnsAsync(new Common.Models.CommandResponse { Succeeded = true });

            _serviceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], new Metadata()));

            var cut = RenderWithMessageComponent();

            var method = typeof(ProductCategoriesOverview).GetMethod( "DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "DeleteCoreAsync method not found via reflection.");

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            _messageMock.Verify(m => m.ShowInfo("Data has been deleted successfully"), Times.Once);
            _serviceMock.Verify(s => s.DeleteAsync(category.Id), Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_WhenDeleteFails_ShowsError()
        {
            var category = new ProductCategoryModel { Id = 7 };
            var response = new Common.Models.CommandResponse
            {
                Succeeded = false,
                Message = "delete failed"
            };

            _serviceMock
                .Setup(s => s.DeleteAsync(category.Id))
                .ReturnsAsync(response);

            var cut = RenderWithMessageComponent();

            var method = typeof(ProductCategoriesOverview).GetMethod( "DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            _messageMock.Verify(m => m.ShowError("delete failed"), Times.Once);
            _serviceMock.Verify(s => s.DeleteAsync(category.Id), Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_WhenResponseIsNull_ShowsGenericError()
        {
            var category = new ProductCategoryModel { Id = 9 };

            _serviceMock
                .Setup(s => s.DeleteAsync(category.Id))
                .ReturnsAsync((Common.Models.CommandResponse?)null);

            var cut = RenderWithMessageComponent();

            var method = typeof(ProductCategoriesOverview).GetMethod( "DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            _messageMock.Verify(
                m => m.ShowError("Delete failed. Please try again."),
                Times.Once);
        }

        // ---------- DataProviderAsync ----------
        [Test]
        public async Task DataProviderAsync_CallsService_SavesQuery_InSessionStorage_AndReturnsData()
        {
            var items = new List<ProductCategoryModel>
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

            var paged = new PagedList<ProductCategoryModel>(items, metadata);

            _serviceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(paged);

            var cut = RenderWithMessageComponent();

            var method = typeof(ProductCategoriesOverview).GetMethod("DataProviderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var request = new GridDataProviderRequest<ProductCategoryModel>
            {
                Filters = null,
                Sorting = null,
                PageNumber = 1,
                PageSize = 10
            };

            var result = await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<ProductCategoryModel>>)method!
                    .Invoke(cut.Instance, [request])!;
                return await task;
            });

            _serviceMock.Verify(
                s => s.SearchAsync(It.Is<QueryParameters<ProductCategoryModel>>(q =>
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