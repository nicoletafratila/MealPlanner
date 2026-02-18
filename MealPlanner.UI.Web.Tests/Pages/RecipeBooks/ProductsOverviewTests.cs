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
    public class ProductsOverviewTests
    {
        private const string EditBaseUrl = "recipebooks/productedit/";

        private BunitContext _ctx = null!;
        private Mock<IProductService> _productServiceMock = null!;
        private Mock<ISessionStorageService> _sessionStorageMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _productServiceMock = new Mock<IProductService>(MockBehavior.Strict);
            _sessionStorageMock = new Mock<ISessionStorageService>(MockBehavior.Strict);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_productServiceMock.Object);
            _ctx.Services.AddSingleton(_sessionStorageMock.Object);

            _ctx.Services.AddScoped<BreadcrumbService>();
            _ctx.Services.AddScoped<ModalService>();
            _ctx.Services.AddScoped<PreloadService>();

            _ctx.JSInterop.SetupVoid("window.blazorBootstrap.dropdown.initialize", _ => true);
            _ctx.JSInterop.SetupVoid("window.blazorBootstrap.confirmDialog.show", _ => true);

            _productServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductModel>>()))
                .ReturnsAsync(new PagedList<ProductModel>([], new Metadata()));

            _sessionStorageMock
                .Setup(s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
            _productServiceMock.Reset();
            _sessionStorageMock.Reset();
            _messageComponentMock.Reset();
        }

        private IRenderedComponent<ProductsOverview> RenderWithMessageComponent()
        {
            return _ctx.Render<ProductsOverview>(parameters =>
            {
                parameters.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
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
                var m = typeof(ProductsOverview).GetMethod("New", BindingFlags.Instance | BindingFlags.NonPublic);
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

            var method = typeof(ProductsOverview).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var model = new ProductModel { Id = 42 };

            cut.InvokeAsync(() => method!.Invoke(cut.Instance, [model]));

            Assert.That(navManager.Uri, Does.EndWith($"{EditBaseUrl}42"));
        }

        // ---------- DeleteCoreAsync (core delete logic) ----------
        [Test]
        public async Task DeleteCoreAsync_WhenDeleteSucceeds_ShowsInfo_AndRefreshesGrid()
        {
            var product = new ProductModel { Id = 5 };

            _productServiceMock
                .Setup(s => s.DeleteAsync(product.Id))
                .ReturnsAsync(new Common.Models.CommandResponse { Succeeded = true });

            var cut = RenderWithMessageComponent();

            var method = typeof(ProductsOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "DeleteCoreAsync method not found via reflection.");

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been deleted successfully"),
                Times.Once);

            _productServiceMock.Verify(s => s.DeleteAsync(product.Id), Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_WhenDeleteFails_ShowsError()
        {
            var product = new ProductModel { Id = 7 };
            var response = new Common.Models.CommandResponse
            {
                Succeeded = false,
                Message = "delete failed"
            };

            _productServiceMock
                .Setup(s => s.DeleteAsync(product.Id))
                .ReturnsAsync(response);

            var cut = RenderWithMessageComponent();

            var method = typeof(ProductsOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            _messageComponentMock.Verify(m => m.ShowError("delete failed"), Times.Once);
            _productServiceMock.Verify(s => s.DeleteAsync(product.Id), Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_WhenResponseIsNull_ShowsGenericError()
        {
            var product = new ProductModel { Id = 9 };

            _productServiceMock
                .Setup(s => s.DeleteAsync(product.Id))
                .ReturnsAsync((Common.Models.CommandResponse?)null);

            var cut = RenderWithMessageComponent();

            var method = typeof(ProductsOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            _messageComponentMock.Verify(
                m => m.ShowError("Delete failed. Please try again."),
                Times.Once);
        }

        // ---------- DataProviderAsync ----------
        [Test]
        public async Task DataProviderAsync_CallsService_SavesQuery_InSessionStorage_AndReturnsData()
        {
            var items = new List<ProductModel>
            {
                new() { Id = 1 },
                new() { Id = 2 },
            };

            var metadata = new Metadata
            {
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 2
            };

            var paged = new PagedList<ProductModel>(items, metadata);

            _productServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductModel>>()))
                .ReturnsAsync(paged);

            var cut = RenderWithMessageComponent();

            var method = typeof(ProductsOverview).GetMethod("DataProviderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var request = new GridDataProviderRequest<ProductModel>
            {
                Filters = null,
                Sorting = null,
                PageNumber = 1,
                PageSize = 10
            };

            var result = await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<ProductModel>>)method!.Invoke(cut.Instance, [request])!;
                return await task;
            });

            _productServiceMock.Verify(
                s => s.SearchAsync(It.Is<QueryParameters<ProductModel>>(q =>
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