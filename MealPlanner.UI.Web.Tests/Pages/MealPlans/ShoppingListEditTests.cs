using System.Reflection;
using Blazored.Modal;
using Bunit;
using Common.Models;
using Common.Pagination;
using Common.UI;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Pages.MealPlans;
using MealPlanner.UI.Web.Services.MealPlans;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Tests.Pages.MealPlans
{
    [TestFixture]
    public class ShoppingListEditTests
    {
        private BunitContext _ctx = null!;
        private Mock<IShoppingListService> _shoppingListServiceMock = null!;
        private Mock<IProductCategoryService> _productCategoryServiceMock = null!;
        private Mock<IProductService> _productServiceMock = null!;
        private Mock<IShopService> _shopServiceMock = null!;
        private Mock<IMealPlanService> _mealPlanServiceMock = null!;
        private Mock<IRecipeService> _recipeServiceMock = null!;
        private Mock<IUnitService> _unitServiceMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _shoppingListServiceMock = new Mock<IShoppingListService>(MockBehavior.Strict);
            _productCategoryServiceMock = new Mock<IProductCategoryService>(MockBehavior.Strict);
            _productServiceMock = new Mock<IProductService>(MockBehavior.Strict);
            _shopServiceMock = new Mock<IShopService>(MockBehavior.Strict);
            _mealPlanServiceMock = new Mock<IMealPlanService>(MockBehavior.Strict);
            _recipeServiceMock = new Mock<IRecipeService>(MockBehavior.Strict);
            _unitServiceMock = new Mock<IUnitService>(MockBehavior.Strict);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_shoppingListServiceMock.Object);
            _ctx.Services.AddSingleton(_productCategoryServiceMock.Object);
            _ctx.Services.AddSingleton(_productServiceMock.Object);
            _ctx.Services.AddSingleton(_shopServiceMock.Object);
            _ctx.Services.AddSingleton(_mealPlanServiceMock.Object);
            _ctx.Services.AddSingleton(_recipeServiceMock.Object);
            _ctx.Services.AddSingleton(_unitServiceMock.Object);
            _ctx.Services.AddSingleton(_messageComponentMock.Object);

            _ctx.Services.AddBlazorBootstrap();
            _ctx.Services.AddBlazoredModal();
            _ctx.Services.AddLogging();

            _ctx.JSInterop.SetupVoid("checkQuantity", _ => true).SetVoidResult();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
        }

        private void ArrangeLookups()
        {
            _productCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], new Metadata()));

            _shopServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>()))
                .ReturnsAsync(new PagedList<ShopModel>([], new Metadata()));

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>()))
                .ReturnsAsync(new PagedList<UnitModel>([], new Metadata()));
        }

        private IRenderedComponent<ShoppingListEdit> RenderComponent(string? id = null)
        {
            return _ctx.Render<ShoppingListEdit>(ps =>
            {
                if (id is not null)
                    ps.Add(p => p.Id, id);

                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
            });
        }

        // ---------- OnInitializedAsync ----------
        [Test]
        public void OnInitializedAsync_WithIdZero_CreatesNewShoppingList()
        {
            // Arrange
            ArrangeLookups();

            // Act
            var cut = RenderComponent("0");

            // Assert
            Assert.That(cut.Instance.ShoppingList, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(cut.Instance.ShoppingList!.Id, Is.EqualTo(0));
                Assert.That(cut.Instance.ShoppingList!.Products, Is.Not.Null);
            });

            _shoppingListServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public void OnInitializedAsync_WithNonZeroId_LoadsShoppingList()
        {
            // Arrange
            ArrangeLookups();

            var existing = new ShoppingListEditModel
            {
                Id = 5,
                ShopId = 1,
                Products = []
            };

            _shoppingListServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _shopServiceMock
                .Setup(s => s.GetEditAsync(1))
                .ReturnsAsync(new ShopEditModel());

            // Act
            var cut = RenderComponent("5");

            // Assert
            Assert.That(cut.Instance.ShoppingList, Is.Not.Null);
            Assert.That(cut.Instance.ShoppingList!.Id, Is.EqualTo(5));

            _shoppingListServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
        }

        // ---------- SaveCoreAsync ----------
        [Test]
        public async Task SaveCoreAsync_AddsShoppingList_WhenIdIsZero()
        {
            // Arrange
            ArrangeLookups();

            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            _shoppingListServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ShoppingListEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent("0");

            var model = new ShoppingListEditModel { Id = 0, Name = "New List" };

            var method = typeof(ShoppingListEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [model])!;
                await task;
            });

            // Assert
            _shoppingListServiceMock.Verify(
                s => s.AddAsync(It.Is<ShoppingListEditModel>(m => m.Name == "New List")),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been saved successfully"),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("mealplans/shoppinglistsoverview"));
        }

        [Test]
        public async Task SaveCoreAsync_UpdatesShoppingList_WhenIdIsNonZero()
        {
            // Arrange
            ArrangeLookups();

            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            var existingShoppingList = new ShoppingListEditModel
            {
                Id = 5,
                ShopId = 1,
                Products = []
            };

            var existingShop = new ShopEditModel
            {
                Id = 1,
            };

            _shopServiceMock
               .Setup(s => s.GetEditAsync(1))
               .ReturnsAsync(existingShop);

            _shoppingListServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existingShoppingList);

            _shoppingListServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<ShoppingListEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent("5");

            var model = new ShoppingListEditModel { Id = 5, Name = "Updated" };

            var method = typeof(ShoppingListEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [model])!;
                await task;
            });

            // Assert
            _shopServiceMock.Verify(s => s.GetEditAsync(1), Times.Once); 
            _shoppingListServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _shoppingListServiceMock.Verify(
                s => s.UpdateAsync(It.Is<ShoppingListEditModel>(m => m.Id == 5)),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been saved successfully"),
                Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            ArrangeLookups();

            _shoppingListServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ShoppingListEditModel>()))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent("0");

            var model = new ShoppingListEditModel { Id = 0, Name = "New List" };

            var method = typeof(ShoppingListEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [model])!;
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
            ArrangeLookups();

            var response = new CommandResponse
            {
                Succeeded = false,
                Message = "Validation error"
            };

            _shoppingListServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ShoppingListEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent("0");

            var model = new ShoppingListEditModel { Id = 0, Name = "New List" };

            var method = typeof(ShoppingListEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [model])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowError("Validation error"),
                Times.Once);
        }

        // ---------- DeleteAsync / DeleteCoreAsync ----------
        [Test]
        public async Task DeleteAsync_DoesNothing_WhenIdIsZero()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Id = 0 };

            var method = typeof(ShoppingListEdit).GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _shoppingListServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public async Task DeleteCoreAsync_Deletes_WhenResponseSucceeded()
        {
            // Arrange
            ArrangeLookups();

            var response = new CommandResponse
            {
                Succeeded = true,
                Message = "ok"
            };

            var existingShoppingList = new ShoppingListEditModel
            {
                Id = 5,
                ShopId = 1,
                Products = []
            };

            var existingShop = new ShopEditModel
            {
                Id = 1,
            };

            _shopServiceMock
               .Setup(s => s.GetEditAsync(1))
               .ReturnsAsync(existingShop);

            _shoppingListServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existingShoppingList);

            _shoppingListServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync(response);

            var cut = RenderComponent("5");

            var model = new ShoppingListEditModel { Id = 5 };

            var method = typeof(ShoppingListEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [model])!;
                await task;
            });

            // Assert
            _shopServiceMock.Verify(s => s.GetEditAsync(1), Times.Once); 
            _shoppingListServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _shoppingListServiceMock.Verify(s => s.DeleteAsync(5), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been deleted successfully"),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("mealplans/shoppinglistsoverview"));
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            ArrangeLookups();

            var existingShoppingList = new ShoppingListEditModel
            {
                Id = 5,
                ShopId = 1,
                Products = []
            };

            var existingShop = new ShopEditModel
            {
                Id = 1,
            };

            _shopServiceMock
               .Setup(s => s.GetEditAsync(1))
               .ReturnsAsync(existingShop);

            _shoppingListServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existingShoppingList);

            _shoppingListServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent("5");

            var model = new ShoppingListEditModel { Id = 5 };

            var method = typeof(ShoppingListEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [model])!;
                await task;
            });

            // Assert
            _shopServiceMock.Verify(s => s.GetEditAsync(1), Times.Once); 
            _shoppingListServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowError("Delete failed. Please try again."),
                Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsResponseMessage_WhenFailed()
        {
            // Arrange
            ArrangeLookups();

            var response = new CommandResponse
            {
                Succeeded = false,
                Message = "Delete failed because of dependency"
            };

            var existingShoppingList = new ShoppingListEditModel
            {
                Id = 5,
                ShopId = 1,
                Products = []
            };

            var existingShop = new ShopEditModel
            {
                Id = 1,
            };

            _shopServiceMock
               .Setup(s => s.GetEditAsync(1))
               .ReturnsAsync(existingShop);

            _shoppingListServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existingShoppingList);

            _shoppingListServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync(response);

            var cut = RenderComponent("5");

            var model = new ShoppingListEditModel { Id = 5 };

            var method = typeof(ShoppingListEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [model])!;
                await task;
            });

            // Assert
            _shopServiceMock.Verify(s => s.GetEditAsync(1), Times.Once); 
            _shoppingListServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowError("Delete failed because of dependency"),
                Times.Once);
        }

        // ---------- CanAddProduct ----------
        [Test]
        public void CanAddProduct_False_WhenRequiredFieldsMissing()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");

            cut.Instance.ProductId = "0";
            cut.Instance.Quantity = "1";

            var prop = typeof(ShoppingListEdit).GetProperty("CanAddProduct", BindingFlags.Instance | BindingFlags.NonPublic);
            var result = (bool)prop!.GetValue(cut.Instance)!;

            Assert.That(result, Is.False);
        }

        [Test]
        public void CanAddProduct_True_WhenAllFieldsValid()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");

            cut.Instance.ProductId = "10";
            cut.Instance.Quantity = "2.5";

            var prop = typeof(ShoppingListEdit).GetProperty("CanAddProduct", BindingFlags.Instance | BindingFlags.NonPublic);
            var result = (bool)prop!.GetValue(cut.Instance)!;

            Assert.That(result, Is.True);
        }

        // ---------- OnProductCategoryChangedAsync ----------
        [Test]
        public async Task OnProductCategoryChangedAsync_BuildsFilters_AndResetsFields()
        {
            // Arrange
            ArrangeLookups();

            var products = new PagedList<ProductModel>([new() { Id = 1 }], new Metadata());

            _productServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductModel>>()))
                .ReturnsAsync(products);

            var cut = RenderComponent("0");

            var method = typeof(ShoppingListEdit).GetMethod("OnProductCategoryChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var args = new ChangeEventArgs { Value = "3" };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [args])!;
                await task;
            });

            // Assert
            _productServiceMock.Verify(
                s => s.SearchAsync(It.Is<QueryParameters<ProductModel>>(qp =>
                    qp.Filters != null &&
                    qp.Filters.Count() == 1 &&
                    qp.Filters.First().PropertyName == "ProductCategoryId" &&
                    (string)qp.Filters.First().Value == "3")),
                Times.Once);

            Assert.Multiple(() =>
            {
                Assert.That(cut.Instance.ProductId, Is.EqualTo(string.Empty));
                Assert.That(cut.Instance.Quantity, Is.EqualTo(string.Empty));
            });
        }

        // ---------- CheckQuantityAsync ----------
        [Test]
        public async Task CheckQuantityAsync_InvokesJsFunction()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");

            var method = typeof(ShoppingListEdit).GetMethod("CheckQuantityAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var args = new ChangeEventArgs { Value = "1.0" };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [args])!;
                await task;
            });
        }
    }
}