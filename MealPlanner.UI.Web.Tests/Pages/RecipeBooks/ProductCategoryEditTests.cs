using System.Reflection;
using Bunit;
using Common.Models;
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
            Assert.That(cut.Instance.ProductCategory.Id, Is.EqualTo(0));

            _productCategoryServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public void OnInitializedAsync_WithValidId_LoadsProductCategory()
        {
            // Arrange
            var existing = new ProductCategoryEditModel
            {
                Id = 5,
                Name = "Loaded Category"
            };

            _productCategoryServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            // Act
            var cut = RenderComponent(id: "5");

            // Assert
            Assert.That(cut.Instance.ProductCategory.Id, Is.EqualTo(5));
            Assert.That(cut.Instance.ProductCategory.Name, Is.EqualTo("Loaded Category"));

            _productCategoryServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
        }

        [Test]
        public void OnInitializedAsync_WithValidId_NullFromService_FallsBackToCategoryWithId()
        {
            // Arrange
            _productCategoryServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync((ProductCategoryEditModel?)null);

            // Act
            var cut = RenderComponent(id: "5");

            // Assert
            Assert.That(cut.Instance.ProductCategory.Id, Is.EqualTo(5));
            _productCategoryServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
        }

        // ---------- SaveCoreAsync ----------
        [Test]
        public async Task SaveCoreAsync_AddsProductCategory_WhenIdIsZero()
        {
            // Arrange
            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            _productCategoryServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ProductCategoryEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: "0");

            var category = new ProductCategoryEditModel { Id = 0, Name = "New Category" };

            var method = typeof(ProductCategoryEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { category })!;
                await task;
            });

            // Assert
            _productCategoryServiceMock.Verify(
                s => s.AddAsync(It.Is<ProductCategoryEditModel>(c => c.Name == "New Category")),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been saved successfully"),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri.EndsWith("recipebooks/productcategoriesoverview"), Is.True);
        }

        [Test]
        public async Task SaveCoreAsync_UpdatesProductCategory_WhenIdIsNonZero()
        {
            // Arrange
            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            var existing = new ProductCategoryEditModel
            {
                Id = 5,
                Name = "Loaded Category"
            };

            _productCategoryServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _productCategoryServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<ProductCategoryEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: "5");

            var category = new ProductCategoryEditModel { Id = 5, Name = "Updated Category" };

            var method = typeof(ProductCategoryEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { category })!;
                await task;
            });

            // Assert
            _productCategoryServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _productCategoryServiceMock.Verify(
                s => s.UpdateAsync(It.Is<ProductCategoryEditModel>(c => c.Id == 5)),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been saved successfully"),
                Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_ShowsGenericError_WhenResponseIsNull()
        {
            // Arrange
            _productCategoryServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ProductCategoryEditModel>()))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent(id: "0");

            var category = new ProductCategoryEditModel { Id = 0, Name = "New Category" };

            var method = typeof(ProductCategoryEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { category })!;
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
            var response = new CommandResponse
            {
                Succeeded = false,
                Message = "Validation error"
            };

            _productCategoryServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ProductCategoryEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: "0");

            var category = new ProductCategoryEditModel { Id = 0, Name = "New Category" };

            var method = typeof(ProductCategoryEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { category })!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowError("Validation error"),
                Times.Once);
        }

        // ---------- DeleteAsync / DeleteCoreAsync ----------
        [Test]
        public async Task DeleteAsync_DoesNothing_WhenProductCategoryIdIsZero()
        {
            // Arrange
            var cut = RenderComponent(id: "0");
            cut.Instance.ProductCategory = new ProductCategoryEditModel { Id = 0 };

            var method = typeof(ProductCategoryEdit).GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, Array.Empty<object>())!;
                await task;
            });

            // Assert
            _productCategoryServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<int>()),
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

            var existing = new ProductCategoryEditModel
            {
                Id = 5,
                Name = "Loaded Category"
            };

            _productCategoryServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _productCategoryServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: "5");

            var category = new ProductCategoryEditModel { Id = 5, Name = "ToDelete" };

            var method = typeof(ProductCategoryEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { category })!;
                await task;
            });

            // Assert
            _productCategoryServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _productCategoryServiceMock.Verify(s => s.DeleteAsync(5), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been deleted successfully"),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri.EndsWith("recipebooks/productcategoriesoverview"), Is.True);
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            var existing = new ProductCategoryEditModel
            {
                Id = 5,
                Name = "Loaded Category"
            };

            _productCategoryServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _productCategoryServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent(id: "5");

            var category = new ProductCategoryEditModel { Id = 5 };

            var method = typeof(ProductCategoryEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { category })!;
                await task;
            });

            // Assert
            _productCategoryServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
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

            var existing = new ProductCategoryEditModel
            {
                Id = 5,
                Name = "Loaded Category"
            };

            _productCategoryServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _productCategoryServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: "5");

            var category = new ProductCategoryEditModel { Id = 5 };

            var method = typeof(ProductCategoryEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { category })!;
                await task;
            });

            // Assert
            _productCategoryServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowError("Delete failed because of dependency"),
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
            cut.InvokeAsync(() => method!.Invoke(cut.Instance, Array.Empty<object>()));

            // Assert
            Assert.That(nav.Uri.EndsWith("recipebooks/productcategoriesoverview"), Is.True);
        }
    }
}