using System.Reflection;using BlazorBootstrap; using Blazored.SessionStorage; using Bunit; using Common.Pagination; using Common.UI; using MealPlanner.UI.Web.Pages.RecipeBooks; using Microsoft.AspNetCore.Components; using Microsoft.Extensions.DependencyInjection; using Moq; using RecipeBook.Services.Http; using RecipeBook.Shared.Models;

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
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductModel>([], new Metadata()));

            _sessionStorageMock
                .Setup(s => s.GetItemAsync<string?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

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

            var productId = Guid.NewGuid();
            var model = new ProductModel { Id = productId };

            cut.InvokeAsync(() => method!.Invoke(cut.Instance, [model]));

            Assert.That(navManager.Uri, Does.EndWith($"{EditBaseUrl}{productId}"));
        }

        // ---------- DeleteCoreAsync ----------
        [Test]
        public async Task DeleteCoreAsync_WhenDeleteSucceeds_ShowsInfo_AndRefreshesGrid()
        {
            var product = new ProductModel { Id = Guid.NewGuid() };

            _productServiceMock
                .Setup(s => s.DeleteAsync(product.Id, CancellationToken.None))
                .ReturnsAsync(new Common.Models.CommandResponse { Succeeded = true });

            var cut = RenderWithMessageComponent();

            var method = typeof(ProductsOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            _messageComponentMock.Verify(m => m.ShowInfoAsync("Data has been deleted successfully", It.IsAny<string>(), CancellationToken.None), Times.Once);
            _productServiceMock.Verify(s => s.DeleteAsync(product.Id, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_WhenDeleteFails_ShowsError()
        {
            var product = new ProductModel { Id = Guid.NewGuid() };
            var response = new Common.Models.CommandResponse { Succeeded = false, Message = "delete failed" };

            _productServiceMock.Setup(s => s.DeleteAsync(product.Id, CancellationToken.None)).ReturnsAsync(response);

            var cut = RenderWithMessageComponent();

            var method = typeof(ProductsOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            _messageComponentMock.Verify(m => m.ShowErrorAsync("delete failed", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None), Times.Once);
            _productServiceMock.Verify(s => s.DeleteAsync(product.Id, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_WhenResponseIsNull_ShowsGenericError()
        {
            var product = new ProductModel { Id = Guid.NewGuid() };

            _productServiceMock.Setup(s => s.DeleteAsync(product.Id, CancellationToken.None)).ReturnsAsync((Common.Models.CommandResponse?)null);

            var cut = RenderWithMessageComponent();

            var method = typeof(ProductsOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            _messageComponentMock.Verify(m => m.ShowErrorAsync("Delete failed. Please try again.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None), Times.Once);
        }

        // ---------- DataProviderAsync ----------
        [Test]
        public async Task DataProviderAsync_CallsService_SavesQuery_InSessionStorage_AndReturnsData()
        {
            var items = new List<ProductModel> { new() { Id = Guid.NewGuid() }, new() { Id = Guid.NewGuid() } };
            var paged = new PagedList<ProductModel>(items, new Metadata { PageNumber = 1, PageSize = 20, TotalCount = 2 });

            _productServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductModel>>(), CancellationToken.None))
                .ReturnsAsync(paged);

            var cut = RenderWithMessageComponent();

            var method = typeof(ProductsOverview).GetMethod("DataProviderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var request = new GridDataProviderRequest<ProductModel> { PageNumber = 1, PageSize = 20 };

            var result = await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<ProductModel>>)method!.Invoke(cut.Instance, [request])!;
                return await task;
            });

            _productServiceMock.Verify(
                s => s.SearchAsync(It.Is<QueryParameters<ProductModel>>(q => q.PageNumber == 1 && q.PageSize == 20), CancellationToken.None),
                Times.Exactly(2));

            _sessionStorageMock.Verify(s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Data!.Count(), Is.EqualTo(2));
                Assert.That(result.TotalCount, Is.EqualTo(2));
            }
        }
    }
}
