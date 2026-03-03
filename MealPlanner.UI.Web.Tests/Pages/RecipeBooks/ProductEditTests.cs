using System.Reflection;
using Bunit;
using Common.Models;
using Common.Pagination;
using Common.UI;
using MealPlanner.UI.Web.Pages.RecipeBooks;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RecipeBook.Shared.Models;

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
                if (id is not null)
                {
                    ps.Add(p => p.Id, id);
                }

                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
            });
        }

        // ---------- OnInitializedAsync ----------
        [Test]
        public void OnInitializedAsync_WithNullOrZeroId_CreatesNewProduct()
        {
            // Arrange
            var units = new PagedList<UnitModel>([], new Metadata());
            var categories = new PagedList<ProductCategoryModel>([], new Metadata());

            _unitServiceMock.Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>())).ReturnsAsync(units);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories);

            // Act
            var cut = RenderComponent(id: "0");

            // Assert
            Assert.That(cut.Instance.Product, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cut.Instance.Product.Id, Is.Zero);
                Assert.That(cut.Instance.Units, Is.SameAs(units));
                Assert.That(cut.Instance.Categories, Is.SameAs(categories));
            }

            _productServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public void OnInitializedAsync_WithValidId_LoadsProduct()
        {
            // Arrange
            var units = new PagedList<UnitModel>([], new Metadata());
            var categories = new PagedList<ProductCategoryModel>([], new Metadata());

            _unitServiceMock.Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>())).ReturnsAsync(units);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories);

            var existing = new ProductEditModel
            {
                Id = 5,
                Name = "Loaded Product"
            };

            _productServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            // Act
            var cut = RenderComponent(id: "5");

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(cut.Instance.Product.Id, Is.EqualTo(5));
                Assert.That(cut.Instance.Product.Name, Is.EqualTo("Loaded Product"));
            }

            _productServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
        }

        [Test]
        public void OnInitializedAsync_WithValidId_NullFromService_FallsBackToProductWithId()
        {
            // Arrange
            var units = new PagedList<UnitModel>([], new Metadata());
            var categories = new PagedList<ProductCategoryModel>([], new Metadata());

            _unitServiceMock.Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>())).ReturnsAsync(units);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories);

            _productServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync((ProductEditModel?)null);

            // Act
            var cut = RenderComponent(id: "5");

            // Assert
            Assert.That(cut.Instance.Product.Id, Is.EqualTo(5));
            _productServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
        }

        // ---------- SaveCoreAsync ----------
        [Test]
        public async Task SaveCoreAsync_AddsProduct_WhenIdIsZero()
        {
            // Arrange
            var units = new PagedList<UnitModel>([], new Metadata());
            var categories = new PagedList<ProductCategoryModel>([], new Metadata());
            _unitServiceMock.Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>())).ReturnsAsync(units);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories);

            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            _productServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ProductEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: "0");

            var product = new ProductEditModel { Id = 0, Name = "New Product" };

            var method = typeof(ProductEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            // Assert
            _productServiceMock.Verify(
                s => s.AddAsync(It.Is<ProductEditModel>(p => p.Name == "New Product")),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been saved successfully"),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/productsoverview"));
        }

        [Test]
        public async Task SaveCoreAsync_UpdatesProduct_WhenIdIsNonZero()
        {
            // Arrange
            var units = new PagedList<UnitModel>([], new Metadata());
            var categories = new PagedList<ProductCategoryModel>([], new Metadata());
            _unitServiceMock.Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>())).ReturnsAsync(units);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories);

            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            var existing = new ProductEditModel
            {
                Id = 5,
                Name = "Loaded Product"
            };

            _productServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _productServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<ProductEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: "5");

            var product = new ProductEditModel { Id = 5, Name = "Updated Product" };

            var method = typeof(ProductEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            // Assert
            _productServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _productServiceMock.Verify(
                s => s.UpdateAsync(It.Is<ProductEditModel>(p => p.Id == 5)),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been saved successfully"),
                Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_ShowsGenericError_WhenResponseIsNull()
        {
            // Arrange
            var units = new PagedList<UnitModel>([], new Metadata());
            var categories = new PagedList<ProductCategoryModel>([], new Metadata());
            _unitServiceMock.Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>())).ReturnsAsync(units);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories);

            _productServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ProductEditModel>()))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent(id: "0");

            var product = new ProductEditModel { Id = 0, Name = "New Product" };

            var method = typeof(ProductEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowError("Save failed. Please try again."),
                Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_ShowsResponseMessage_WhenFailed()
        {
            // Arrange
            var units = new PagedList<UnitModel>([], new Metadata());
            var categories = new PagedList<ProductCategoryModel>([], new Metadata());
            _unitServiceMock.Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>())).ReturnsAsync(units);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories);

            var response = new CommandResponse
            {
                Succeeded = false,
                Message = "Validation error"
            };

            _productServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ProductEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: "0");

            var product = new ProductEditModel { Id = 0, Name = "New Product" };

            var method = typeof(ProductEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowError("Validation error"),
                Times.Once);
        }

        // ---------- DeleteAsync / DeleteCoreAsync ----------
        [Test]
        public async Task DeleteAsync_DoesNothing_WhenProductIdIsZero()
        {
            // Arrange
            var units = new PagedList<UnitModel>([], new Metadata());
            var categories = new PagedList<ProductCategoryModel>([], new Metadata());
            _unitServiceMock.Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>())).ReturnsAsync(units);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories);

            var cut = RenderComponent(id: "0");
            cut.Instance.Product = new ProductEditModel { Id = 0 };

            var method = typeof(ProductEdit).GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _productServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public async Task DeleteCoreAsync_Deletes_WhenResponseSucceeded()
        {
            // Arrange
            var units = new PagedList<UnitModel>([], new Metadata());
            var categories = new PagedList<ProductCategoryModel>([], new Metadata());
            _unitServiceMock.Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>())).ReturnsAsync(units);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories);

            var response = new CommandResponse
            {
                Succeeded = true,
                Message = "ok"
            };

            var existing = new ProductEditModel
            {
                Id = 5,
                Name = "Loaded Product"
            };

            _productServiceMock
               .Setup(s => s.GetEditAsync(5))
               .ReturnsAsync(existing);

            _productServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: "5");

            var product = new ProductEditModel { Id = 5, Name = "ToDelete" };

            var method = typeof(ProductEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            // Assert
            _productServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _productServiceMock.Verify(s => s.DeleteAsync(5), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been deleted successfully"),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/productsoverview"));
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            var units = new PagedList<UnitModel>([], new Metadata());
            var categories = new PagedList<ProductCategoryModel>([], new Metadata());
            _unitServiceMock.Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>())).ReturnsAsync(units);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories);

            var existing = new ProductEditModel
            {
                Id = 5,
                Name = "Loaded Product"
            };

            _productServiceMock
               .Setup(s => s.GetEditAsync(5))
               .ReturnsAsync(existing);

            _productServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent(id: "5");

            var product = new ProductEditModel { Id = 5 };

            var method = typeof(ProductEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            // Assert
            _productServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowError("Delete failed. Please try again."),
                Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsResponseMessage_WhenFailed()
        {
            // Arrange
            var units = new PagedList<UnitModel>([], new Metadata());
            var categories = new PagedList<ProductCategoryModel>([], new Metadata());
            _unitServiceMock.Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>())).ReturnsAsync(units);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories);

            var response = new CommandResponse
            {
                Succeeded = false,
                Message = "Delete failed because of dependency"
            };

            var existing = new ProductEditModel
            {
                Id = 5,
                Name = "Loaded Product"
            };

            _productServiceMock
               .Setup(s => s.GetEditAsync(5))
               .ReturnsAsync(existing);

            _productServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: "5");

            var product = new ProductEditModel { Id = 5 };

            var method = typeof(ProductEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            // Assert
            _productServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowError("Delete failed because of dependency"),
                Times.Once);
        }

        // ---------- NavigateToOverview ----------
        [Test]
        public void NavigateToOverview_NavigatesToOverviewUrl()
        {
            // Arrange
            var units = new PagedList<UnitModel>([], new Metadata());
            var categories = new PagedList<ProductCategoryModel>([], new Metadata());
            _unitServiceMock.Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>())).ReturnsAsync(units);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories);

            var cut = RenderComponent(id: "0");
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            var method = typeof(ProductEdit).GetMethod("NavigateToOverview", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            cut.InvokeAsync(() => method!.Invoke(cut.Instance, []));

            // Assert
            Assert.That(nav.Uri, Does.EndWith("recipebooks/productsoverview"));
        }

        // ---------- OnInputFileChangeAsync ----------
        [Test]
        public async Task OnInputFileChangeAsync_SetsImageContent_WhenWithinLimit()
        {
            // Arrange
            var units = new PagedList<UnitModel>([], new Metadata());
            var categories = new PagedList<ProductCategoryModel>([], new Metadata());
            _unitServiceMock.Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>())).ReturnsAsync(units);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories);

            var cut = RenderComponent("0");
            cut.Instance.Product = new ProductEditModel { Id = 1 };

            var bytes = new byte[] { 1, 2, 3, 4 };
            var file = new FakeBrowserFile(bytes, "img.png", "image/png");
            var args = new InputFileChangeEventArgs([file]);

            var method = typeof(ProductEdit).GetMethod("OnInputFileChangeAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                Task task = (Task)method!.Invoke(cut.Instance, [args])!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.Product.ImageContent, Is.Not.Null);
            Assert.That(cut.Instance.Product.ImageContent!.SequenceEqual(bytes), Is.True);
        }

        [Test]
        public async Task OnInputFileChangeAsync_ShowsError_WhenFileTooLargeOrError()
        {
            // Arrange
            var units = new PagedList<UnitModel>([], new Metadata());
            var categories = new PagedList<ProductCategoryModel>([], new Metadata());
            _unitServiceMock.Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>())).ReturnsAsync(units);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories);

            var cut = RenderComponent("0");
            cut.Instance.Product = new ProductEditModel { Id = 1 };

            var bigBytes = new byte[1024 * 1024 * 5]; // 5 MB
            var file = new FakeBrowserFile(bigBytes, "big.bin", "application/octet-stream", throwOnOpen: true);
            var args = new InputFileChangeEventArgs([file]);

            var method = typeof(ProductEdit).GetMethod("OnInputFileChangeAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [args])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowError(It.Is<string>(msg => msg.Contains("Maximum allowed size"))),
                Times.Once);
        }
    }
}