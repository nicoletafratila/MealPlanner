using System.Reflection;
using Bunit;
using Common.Models;
using Common.UI;
using MealPlanner.UI.Web.Pages.RecipeBooks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Tests.Pages.RecipeBooks
{
    [TestFixture]
    public class ProductCategoryEditTests
    {
        private BunitContext _ctx = null!;
        private Mock<IProductCategoryService> _productCategoryServiceMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _productCategoryServiceMock = new Mock<IProductCategoryService>(MockBehavior.Strict);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_productCategoryServiceMock.Object);
            _ctx.Services.AddSingleton(_messageComponentMock.Object);

            _ctx.Services.AddBlazorBootstrap();
            _ctx.Services.AddLogging();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
        }

        private IRenderedComponent<ProductCategoryEdit> RenderComponent(string? id = null)
        {
            return _ctx.Render<ProductCategoryEdit>(ps =>
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
        public void OnInitializedAsync_WithNullOrZeroId_CreatesNewProductCategory()
        {
            // Act
            var cut = RenderComponent(id: "0");

            // Assert
            Assert.That(cut.Instance.ProductCategory, Is.Not.Null);
            Assert.That(cut.Instance.ProductCategory.Id, Is.EqualTo(Guid.Empty));

            _productCategoryServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public void OnInitializedAsync_WithValidId_LoadsProductCategory()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new ProductCategoryEditModel
            {
                Id = id,
                Name = "Loaded Category"
            };

            _productCategoryServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);

            // Act
            var cut = RenderComponent(id: id.ToString());

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(cut.Instance.ProductCategory.Id, Is.EqualTo(id));
                Assert.That(cut.Instance.ProductCategory.Name, Is.EqualTo("Loaded Category"));
            }

            _productCategoryServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
        }

        [Test]
        public void OnInitializedAsync_WithValidId_NullFromService_FallsBackToCategoryWithId()
        {
            // Arrange
            var id = Guid.NewGuid();
            _productCategoryServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync((ProductCategoryEditModel?)null);

            // Act
            var cut = RenderComponent(id: id.ToString());

            // Assert
            Assert.That(cut.Instance.ProductCategory.Id, Is.EqualTo(id));
            _productCategoryServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
        }

        // ---------- SaveCoreAsync ----------
        [Test]
        public async Task SaveCoreAsync_AddsProductCategory_WhenIdIsZero()
        {
            // Arrange
            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            _productCategoryServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ProductCategoryEditModel>(), CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: "0");

            var category = new ProductCategoryEditModel { Id = Guid.Empty, Name = "New Category" };

            var method = typeof(ProductCategoryEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            // Assert
            _productCategoryServiceMock.Verify(
                s => s.AddAsync(It.Is<ProductCategoryEditModel>(c => c.Name == "New Category"), CancellationToken.None),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Data has been saved successfully", It.IsAny<string>(), CancellationToken.None),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/productcategoriesoverview"));
        }

        [Test]
        public async Task SaveCoreAsync_UpdatesProductCategory_WhenIdIsNonZero()
        {
            // Arrange
            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            var id = Guid.NewGuid();
            var existing = new ProductCategoryEditModel
            {
                Id = id,
                Name = "Loaded Category"
            };

            _productCategoryServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);

            _productCategoryServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<ProductCategoryEditModel>(), CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: id.ToString());

            var category = new ProductCategoryEditModel { Id = id, Name = "Updated Category" };

            var method = typeof(ProductCategoryEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            // Assert
            _productCategoryServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
            _productCategoryServiceMock.Verify(
                s => s.UpdateAsync(It.Is<ProductCategoryEditModel>(c => c.Id == id), CancellationToken.None),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Data has been saved successfully", It.IsAny<string>(), CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_ShowsGenericError_WhenResponseIsNull()
        {
            // Arrange
            _productCategoryServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ProductCategoryEditModel>(), CancellationToken.None))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent(id: "0");

            var category = new ProductCategoryEditModel { Id = Guid.Empty, Name = "New Category" };

            var method = typeof(ProductCategoryEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Save failed. Please try again.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_ShowsResponseMessage_WhenFailed()
        {
            // Arrange
            var response = new CommandResponse
            {
                Succeeded = false,
                Message = "Validation error"
            };

            _productCategoryServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ProductCategoryEditModel>(), CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: "0");

            var category = new ProductCategoryEditModel { Id = Guid.Empty, Name = "New Category" };

            var method = typeof(ProductCategoryEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Validation error", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }

        // ---------- DeleteAsync / DeleteCoreAsync ----------
        [Test]
        public async Task DeleteAsync_DoesNothing_WhenProductCategoryIdIsZero()
        {
            // Arrange
            var cut = RenderComponent(id: "0");
            cut.Instance.ProductCategory = new ProductCategoryEditModel { Id = Guid.Empty };

            var method = typeof(ProductCategoryEdit).GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _productCategoryServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task DeleteCoreAsync_Deletes_WhenResponseSucceeded()
        {
            // Arrange
            var response = new CommandResponse
            {
                Succeeded = true,
                Message = "ok"
            };

            var id = Guid.NewGuid();
            var existing = new ProductCategoryEditModel
            {
                Id = id,
                Name = "Loaded Category"
            };

            _productCategoryServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);

            _productCategoryServiceMock
                .Setup(s => s.DeleteAsync(id, CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: id.ToString());

            var category = new ProductCategoryEditModel { Id = id, Name = "ToDelete" };

            var method = typeof(ProductCategoryEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            // Assert
            _productCategoryServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
            _productCategoryServiceMock.Verify(s => s.DeleteAsync(id, CancellationToken.None), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Data has been deleted successfully", It.IsAny<string>(), CancellationToken.None),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/productcategoriesoverview"));
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new ProductCategoryEditModel
            {
                Id = id,
                Name = "Loaded Category"
            };

            _productCategoryServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);

            _productCategoryServiceMock
                .Setup(s => s.DeleteAsync(id, CancellationToken.None))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent(id: id.ToString());

            var category = new ProductCategoryEditModel { Id = id };

            var method = typeof(ProductCategoryEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            // Assert
            _productCategoryServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
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

            var id = Guid.NewGuid();
            var existing = new ProductCategoryEditModel
            {
                Id = id,
                Name = "Loaded Category"
            };

            _productCategoryServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);

            _productCategoryServiceMock
                .Setup(s => s.DeleteAsync(id, CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: id.ToString());

            var category = new ProductCategoryEditModel { Id = id };

            var method = typeof(ProductCategoryEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            // Assert
            _productCategoryServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Delete failed because of dependency", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }

        // ---------- NavigateToOverview ----------
        [Test]
        public void NavigateToOverview_NavigatesToOverviewUrl()
        {
            // Arrange
            var cut = RenderComponent(id: "0");
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            var method = typeof(ProductCategoryEdit).GetMethod("NavigateToOverview", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            cut.InvokeAsync(() => method!.Invoke(cut.Instance, []));

            // Assert
            Assert.That(nav.Uri, Does.EndWith("recipebooks/productcategoriesoverview"));
        }
    }
}