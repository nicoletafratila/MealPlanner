using System.Reflection;using Bunit; using Common.Models; using Common.Pagination; using Common.UI; using MealPlanner.UI.Web.Pages.RecipeBooks; using Microsoft.AspNetCore.Components.Forms; using Microsoft.AspNetCore.Components; using Microsoft.Extensions.DependencyInjection; using Moq; using RecipeBook.Services.Http; using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Tests.Pages.RecipeBooks
{
    [TestFixture]
    public class ProductEditTests
    {
        private BunitContext _ctx = null!;
        private Mock<IProductService> _productServiceMock = null!;
        private Mock<IProductCategoryService> _categoryServiceMock = null!;
        private Mock<IUnitService> _unitServiceMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _productServiceMock = new Mock<IProductService>(MockBehavior.Strict);
            _categoryServiceMock = new Mock<IProductCategoryService>(MockBehavior.Strict);
            _unitServiceMock = new Mock<IUnitService>(MockBehavior.Strict);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_productServiceMock.Object);
            _ctx.Services.AddSingleton(_categoryServiceMock.Object);
            _ctx.Services.AddSingleton(_unitServiceMock.Object);
            _ctx.Services.AddSingleton(_messageComponentMock.Object);

            _ctx.Services.AddBlazorBootstrap();
            _ctx.Services.AddLogging();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
        }

        private IRenderedComponent<ProductEdit> RenderComponent(string? id = null)
        {
            return _ctx.Render<ProductEdit>(ps =>
            {
                if (id is not null) ps.Add(p => p.Id, id);
                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
            });
        }

        private void SetupDefaultServices()
        {
            _unitServiceMock.Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<UnitModel>([], new Metadata()));
            _categoryServiceMock.Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], new Metadata()));
        }

        // ---------- OnInitializedAsync ----------
        [Test]
        public void OnInitializedAsync_WithNullOrEmptyId_CreatesNewProduct()
        {
            SetupDefaultServices();
            var cut = RenderComponent(id: Guid.Empty.ToString());

            Assert.That(cut.Instance.Product, Is.Not.Null);
            Assert.That(cut.Instance.Product.Id, Is.EqualTo(Guid.Empty));

            _productServiceMock.Verify(s => s.GetEditAsync(It.IsAny<Guid>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public void OnInitializedAsync_WithValidId_LoadsProduct()
        {
            SetupDefaultServices();
            var id = Guid.NewGuid();
            var existing = new ProductEditModel { Id = id, Name = "Loaded Product" };

            _productServiceMock.Setup(s => s.GetEditAsync(id, CancellationToken.None)).ReturnsAsync(existing);

            var cut = RenderComponent(id: id.ToString());

            using (Assert.EnterMultipleScope())
            {
                Assert.That(cut.Instance.Product.Id, Is.EqualTo(id));
                Assert.That(cut.Instance.Product.Name, Is.EqualTo("Loaded Product"));
            }

            _productServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
        }

        [Test]
        public void OnInitializedAsync_WithValidId_NullFromService_FallsBackToProductWithId()
        {
            SetupDefaultServices();
            var id = Guid.NewGuid();

            _productServiceMock.Setup(s => s.GetEditAsync(id, CancellationToken.None)).ReturnsAsync((ProductEditModel?)null);

            var cut = RenderComponent(id: id.ToString());

            Assert.That(cut.Instance.Product.Id, Is.EqualTo(id));
            _productServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
        }

        // ---------- SaveCoreAsync ----------
        [Test]
        public async Task SaveCoreAsync_AddsProduct_WhenIdIsEmpty()
        {
            SetupDefaultServices();
            var response = new CommandResponse { Succeeded = true, Message = "ok" };
            _productServiceMock.Setup(s => s.AddAsync(It.IsAny<ProductEditModel>(), CancellationToken.None)).ReturnsAsync(response);

            var cut = RenderComponent(id: Guid.Empty.ToString());
            var product = new ProductEditModel { Id = Guid.Empty, Name = "New Product" };

            var method = typeof(ProductEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            _productServiceMock.Verify(s => s.AddAsync(It.Is<ProductEditModel>(p => p.Name == "New Product"), CancellationToken.None), Times.Once);
            _messageComponentMock.Verify(m => m.ShowInfoAsync("Data has been saved successfully", It.IsAny<string>(), CancellationToken.None), Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/productsoverview"));
        }

        [Test]
        public async Task SaveCoreAsync_UpdatesProduct_WhenIdIsNonEmpty()
        {
            SetupDefaultServices();
            var id = Guid.NewGuid();
            var response = new CommandResponse { Succeeded = true, Message = "ok" };
            var existing = new ProductEditModel { Id = id, Name = "Loaded Product" };

            _productServiceMock.Setup(s => s.GetEditAsync(id, CancellationToken.None)).ReturnsAsync(existing);
            _productServiceMock.Setup(s => s.UpdateAsync(It.IsAny<ProductEditModel>(), CancellationToken.None)).ReturnsAsync(response);

            var cut = RenderComponent(id: id.ToString());
            var product = new ProductEditModel { Id = id, Name = "Updated Product" };

            var method = typeof(ProductEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            _productServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
            _productServiceMock.Verify(s => s.UpdateAsync(It.Is<ProductEditModel>(p => p.Id == id), CancellationToken.None), Times.Once);
            _messageComponentMock.Verify(m => m.ShowInfoAsync("Data has been saved successfully", It.IsAny<string>(), CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_ShowsGenericError_WhenResponseIsNull()
        {
            SetupDefaultServices();
            _productServiceMock.Setup(s => s.AddAsync(It.IsAny<ProductEditModel>(), CancellationToken.None)).ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent(id: Guid.Empty.ToString());
            var product = new ProductEditModel { Id = Guid.Empty, Name = "New Product" };

            var method = typeof(ProductEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            _messageComponentMock.Verify(m => m.ShowErrorAsync("Save failed. Please try again.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_ShowsResponseMessage_WhenFailed()
        {
            SetupDefaultServices();
            var response = new CommandResponse { Succeeded = false, Message = "Validation error" };
            _productServiceMock.Setup(s => s.AddAsync(It.IsAny<ProductEditModel>(), CancellationToken.None)).ReturnsAsync(response);

            var cut = RenderComponent(id: Guid.Empty.ToString());
            var product = new ProductEditModel { Id = Guid.Empty, Name = "New Product" };

            var method = typeof(ProductEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            _messageComponentMock.Verify(m => m.ShowErrorAsync("Validation error", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None), Times.Once);
        }

        // ---------- DeleteAsync / DeleteCoreAsync ----------
        [Test]
        public async Task DeleteAsync_DoesNothing_WhenProductIdIsEmpty()
        {
            SetupDefaultServices();
            var cut = RenderComponent(id: Guid.Empty.ToString());
            cut.Instance.Product = new ProductEditModel { Id = Guid.Empty };

            var method = typeof(ProductEdit).GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            _productServiceMock.Verify(s => s.DeleteAsync(It.IsAny<Guid>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task DeleteCoreAsync_Deletes_WhenResponseSucceeded()
        {
            SetupDefaultServices();
            var id = Guid.NewGuid();
            var response = new CommandResponse { Succeeded = true, Message = "ok" };
            var existing = new ProductEditModel { Id = id, Name = "Loaded Product" };

            _productServiceMock.Setup(s => s.GetEditAsync(id, CancellationToken.None)).ReturnsAsync(existing);
            _productServiceMock.Setup(s => s.DeleteAsync(id, CancellationToken.None)).ReturnsAsync(response);

            var cut = RenderComponent(id: id.ToString());
            var product = new ProductEditModel { Id = id, Name = "ToDelete" };

            var method = typeof(ProductEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            _productServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
            _productServiceMock.Verify(s => s.DeleteAsync(id, CancellationToken.None), Times.Once);
            _messageComponentMock.Verify(m => m.ShowInfoAsync("Data has been deleted successfully", It.IsAny<string>(), CancellationToken.None), Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/productsoverview"));
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            SetupDefaultServices();
            var id = Guid.NewGuid();
            var existing = new ProductEditModel { Id = id, Name = "Loaded Product" };

            _productServiceMock.Setup(s => s.GetEditAsync(id, CancellationToken.None)).ReturnsAsync(existing);
            _productServiceMock.Setup(s => s.DeleteAsync(id, CancellationToken.None)).ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent(id: id.ToString());
            var product = new ProductEditModel { Id = id };

            var method = typeof(ProductEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            _productServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
            _messageComponentMock.Verify(m => m.ShowErrorAsync("Delete failed. Please try again.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsResponseMessage_WhenFailed()
        {
            SetupDefaultServices();
            var id = Guid.NewGuid();
            var response = new CommandResponse { Succeeded = false, Message = "Delete failed because of dependency" };
            var existing = new ProductEditModel { Id = id, Name = "Loaded Product" };

            _productServiceMock.Setup(s => s.GetEditAsync(id, CancellationToken.None)).ReturnsAsync(existing);
            _productServiceMock.Setup(s => s.DeleteAsync(id, CancellationToken.None)).ReturnsAsync(response);

            var cut = RenderComponent(id: id.ToString());
            var product = new ProductEditModel { Id = id };

            var method = typeof(ProductEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            _productServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
            _messageComponentMock.Verify(m => m.ShowErrorAsync("Delete failed because of dependency", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None), Times.Once);
        }

        // ---------- NavigateToOverview ----------
        [Test]
        public void NavigateToOverview_NavigatesToOverviewUrl()
        {
            SetupDefaultServices();
            var cut = RenderComponent(id: Guid.Empty.ToString());
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            var method = typeof(ProductEdit).GetMethod("NavigateToOverview", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            cut.InvokeAsync(() => method!.Invoke(cut.Instance, []));

            Assert.That(nav.Uri, Does.EndWith("recipebooks/productsoverview"));
        }

        // ---------- OnInputFileChangeAsync ----------
        [Test]
        public async Task OnInputFileChangeAsync_SetsImageContent_WhenWithinLimit()
        {
            SetupDefaultServices();
            var cut = RenderComponent(Guid.Empty.ToString());
            cut.Instance.Product = new ProductEditModel { Id = Guid.NewGuid() };

            var bytes = new byte[] { 1, 2, 3, 4 };
            var file = new FakeBrowserFile(bytes, "img.png", "image/png");
            var args = new InputFileChangeEventArgs([file]);

            var method = typeof(ProductEdit).GetMethod("OnInputFileChangeAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                Task task = (Task)method!.Invoke(cut.Instance, [args])!;
                await task;
            });

            Assert.That(cut.Instance.Product.ImageContent, Is.Not.Null);
            Assert.That(cut.Instance.Product.ImageContent!.SequenceEqual(bytes), Is.True);
        }

        [Test]
        public async Task OnInputFileChangeAsync_ShowsError_WhenFileTooLargeOrError()
        {
            SetupDefaultServices();
            var cut = RenderComponent(Guid.Empty.ToString());
            cut.Instance.Product = new ProductEditModel { Id = Guid.NewGuid() };

            var bigBytes = new byte[1024 * 1024 * 5];
            var file = new FakeBrowserFile(bigBytes, "big.bin", "application/octet-stream", throwOnOpen: true);
            var args = new InputFileChangeEventArgs([file]);

            var method = typeof(ProductEdit).GetMethod("OnInputFileChangeAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [args])!;
                await task;
            });

            _messageComponentMock.Verify(
                m => m.ShowErrorAsync(It.Is<string>(msg => msg.Contains("Maximum allowed size")), It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }
    }
}
