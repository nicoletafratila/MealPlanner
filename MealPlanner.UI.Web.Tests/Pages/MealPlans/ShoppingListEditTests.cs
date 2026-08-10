using System.Reflection;
using Blazored.Modal;
using Blazored.Modal.Services;
using Bunit;
using Common.Models;
using Common.Pagination;
using Common.UI;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Pages.MealPlans;
using MealPlanner.UI.Web.Pages.RecipeBooks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RecipeBook.Services.Http;
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
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], new Metadata()));

            _shopServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ShopModel>([], new Metadata()));

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<UnitModel>([], new Metadata()));
        }

        private IRenderedComponent<ShoppingListEdit> RenderComponent(string? id = null, IModalService? modalService = null)
        {
            return _ctx.Render<ShoppingListEdit>(ps =>
            {
                if (id is not null)
                    ps.Add(p => p.Id, id);

                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object);

                if (modalService is not null)
                    ps.AddCascadingValue(modalService);
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
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cut.Instance.ShoppingList!.Id, Is.EqualTo(Guid.Empty));
                Assert.That(cut.Instance.ShoppingList!.Products, Is.Not.Null);
            }

            _shoppingListServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public void OnInitializedAsync_WithNonZeroId_LoadsShoppingList()
        {
            // Arrange
            ArrangeLookups();

            var id = Guid.NewGuid();
            var shopId = Guid.NewGuid();
            var existing = new ShoppingListEditModel
            {
                Id = id,
                ShopId = shopId,
                Products = []
            };

            _shoppingListServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);

            _shopServiceMock
                .Setup(s => s.GetEditAsync(shopId, CancellationToken.None))
                .ReturnsAsync(new ShopEditModel());

            // Act
            var cut = RenderComponent(id.ToString());

            // Assert
            Assert.That(cut.Instance.ShoppingList, Is.Not.Null);
            Assert.That(cut.Instance.ShoppingList!.Id, Is.EqualTo(id));

            _shoppingListServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
        }

        // ---------- SaveCoreAsync ----------
        [Test]
        public async Task SaveCoreAsync_AddsShoppingList_WhenIdIsZero()
        {
            // Arrange
            ArrangeLookups();

            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            _shoppingListServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ShoppingListEditModel>(), CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent("0");

            var model = new ShoppingListEditModel { Id = Guid.Empty, Name = "New List" };

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
                s => s.AddAsync(It.Is<ShoppingListEditModel>(m => m.Name == "New List"), CancellationToken.None),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Data has been saved successfully", It.IsAny<string>(), CancellationToken.None),
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

            var id = Guid.NewGuid();
            var shopId = Guid.NewGuid();
            var existingShoppingList = new ShoppingListEditModel
            {
                Id = id,
                ShopId = shopId,
                Products = []
            };

            var existingShop = new ShopEditModel
            {
                Id = shopId,
            };

            _shopServiceMock
               .Setup(s => s.GetEditAsync(shopId, CancellationToken.None))
               .ReturnsAsync(existingShop);

            _shoppingListServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existingShoppingList);

            _shoppingListServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<ShoppingListEditModel>(), CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent(id.ToString());

            var model = new ShoppingListEditModel { Id = id, Name = "Updated" };

            var method = typeof(ShoppingListEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [model])!;
                await task;
            });

            // Assert
            _shopServiceMock.Verify(s => s.GetEditAsync(shopId, CancellationToken.None), Times.Once);
            _shoppingListServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
            _shoppingListServiceMock.Verify(
                s => s.UpdateAsync(It.Is<ShoppingListEditModel>(m => m.Id == id), CancellationToken.None),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Data has been saved successfully", It.IsAny<string>(), CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            ArrangeLookups();

            _shoppingListServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ShoppingListEditModel>(), CancellationToken.None))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent("0");

            var model = new ShoppingListEditModel { Id = Guid.Empty, Name = "New List" };

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
                m => m.ShowErrorAsync("Save failed. Please try again.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
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
                .Setup(s => s.AddAsync(It.IsAny<ShoppingListEditModel>(), CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent("0");

            var model = new ShoppingListEditModel { Id = Guid.Empty, Name = "New List" };

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
                m => m.ShowErrorAsync("Validation error", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }

        // ---------- DeleteAsync / DeleteCoreAsync ----------
        [Test]
        public async Task DeleteAsync_DoesNothing_WhenIdIsZero()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Id = Guid.Empty };

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
                s => s.DeleteAsync(It.IsAny<Guid>(), CancellationToken.None),
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

            var id = Guid.NewGuid();
            var shopId = Guid.NewGuid();
            var existingShoppingList = new ShoppingListEditModel
            {
                Id = id,
                ShopId = shopId,
                Products = []
            };

            var existingShop = new ShopEditModel
            {
                Id = shopId,
            };

            _shopServiceMock
               .Setup(s => s.GetEditAsync(shopId, CancellationToken.None))
               .ReturnsAsync(existingShop);

            _shoppingListServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existingShoppingList);

            _shoppingListServiceMock
                .Setup(s => s.DeleteAsync(id, CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent(id.ToString());

            var model = new ShoppingListEditModel { Id = id };

            var method = typeof(ShoppingListEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [model])!;
                await task;
            });

            // Assert
            _shopServiceMock.Verify(s => s.GetEditAsync(shopId, CancellationToken.None), Times.Once);
            _shoppingListServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
            _shoppingListServiceMock.Verify(s => s.DeleteAsync(id, CancellationToken.None), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Data has been deleted successfully", It.IsAny<string>(), CancellationToken.None),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("mealplans/shoppinglistsoverview"));
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            ArrangeLookups();

            var id = Guid.NewGuid();
            var shopId = Guid.NewGuid();
            var existingShoppingList = new ShoppingListEditModel
            {
                Id = id,
                ShopId = shopId,
                Products = []
            };

            var existingShop = new ShopEditModel
            {
                Id = shopId,
            };

            _shopServiceMock
               .Setup(s => s.GetEditAsync(shopId, CancellationToken.None))
               .ReturnsAsync(existingShop);

            _shoppingListServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existingShoppingList);

            _shoppingListServiceMock
                .Setup(s => s.DeleteAsync(id, CancellationToken.None))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent(id.ToString());

            var model = new ShoppingListEditModel { Id = id };

            var method = typeof(ShoppingListEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [model])!;
                await task;
            });

            // Assert
            _shopServiceMock.Verify(s => s.GetEditAsync(shopId, CancellationToken.None), Times.Once);
            _shoppingListServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Delete failed. Please try again.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
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

            var id = Guid.NewGuid();
            var shopId = Guid.NewGuid();
            var existingShoppingList = new ShoppingListEditModel
            {
                Id = id,
                ShopId = shopId,
                Products = []
            };

            var existingShop = new ShopEditModel
            {
                Id = shopId,
            };

            _shopServiceMock
               .Setup(s => s.GetEditAsync(shopId, CancellationToken.None))
               .ReturnsAsync(existingShop);

            _shoppingListServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existingShoppingList);

            _shoppingListServiceMock
                .Setup(s => s.DeleteAsync(id, CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent(id.ToString());

            var model = new ShoppingListEditModel { Id = id };

            var method = typeof(ShoppingListEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [model])!;
                await task;
            });

            // Assert
            _shopServiceMock.Verify(s => s.GetEditAsync(shopId, CancellationToken.None), Times.Once);
            _shoppingListServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Delete failed because of dependency", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
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

            var products = new PagedList<ProductModel>([new() { Id = Guid.NewGuid() }], new Metadata());

            _productServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductModel>>(), CancellationToken.None, true))
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
                    qp.Filters.First().Value as string == "3"), CancellationToken.None, true),
                Times.Once);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(cut.Instance.ProductId, Is.EqualTo(string.Empty));
                Assert.That(cut.Instance.Quantity, Is.EqualTo(string.Empty));
            }
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

        // ---------- SaveAsync ----------
        [Test]
        public async Task SaveAsync_DoesNothing_WhenShoppingListIsNull()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = null;

            var method = typeof(ShoppingListEdit).GetMethod("SaveAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _shoppingListServiceMock.Verify(
                s => s.AddAsync(It.IsAny<ShoppingListEditModel>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task SaveAsync_DelegatesToSaveCoreAsync_WhenShoppingListIsNotNull()
        {
            // Arrange
            ArrangeLookups();

            var response = new CommandResponse { Succeeded = true, Message = "ok" };
            _shoppingListServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ShoppingListEditModel>(), CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Id = Guid.Empty, Name = "New List" };

            var method = typeof(ShoppingListEdit).GetMethod("SaveAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _shoppingListServiceMock.Verify(
                s => s.AddAsync(It.Is<ShoppingListEditModel>(m => m.Name == "New List"), CancellationToken.None),
                Times.Once);
        }

        // ---------- AddProductAsync (no-arg wrapper) ----------
        [Test]
        public async Task AddProductAsync_DoesNothing_WhenProductIdBlank()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [] };
            cut.Instance.ProductId = string.Empty;

            var method = typeof(ShoppingListEdit).GetMethod("AddProductAsync", BindingFlags.Instance | BindingFlags.NonPublic, [])!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.ShoppingList!.Products, Is.Empty);
        }

        [Test]
        public async Task AddProductAsync_DoesNothing_WhenProductIdIsZero()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [] };
            cut.Instance.ProductId = "0";
            cut.Instance.UnitId = "1";

            var method = typeof(ShoppingListEdit).GetMethod("AddProductAsync", BindingFlags.Instance | BindingFlags.NonPublic, [])!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.ShoppingList!.Products, Is.Empty);
        }

        [Test]
        public async Task AddProductAsync_DoesNothing_WhenUnitIdIsZero()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            var productId = Guid.NewGuid();
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [] };
            cut.Instance.ProductId = productId.ToString();
            cut.Instance.UnitId = "0";

            var method = typeof(ShoppingListEdit).GetMethod("AddProductAsync", BindingFlags.Instance | BindingFlags.NonPublic, [])!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.ShoppingList!.Products, Is.Empty);
        }

        [Test]
        public async Task AddProductAsync_DoesNothing_WhenProductNotFoundInProductsList()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            var productId = Guid.NewGuid();
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [] };
            cut.Instance.Products = new PagedList<ProductModel>([], new Metadata());
            cut.Instance.ProductId = productId.ToString();
            cut.Instance.UnitId = Guid.NewGuid().ToString();
            cut.Instance.Quantity = "2";

            var method = typeof(ShoppingListEdit).GetMethod("AddProductAsync", BindingFlags.Instance | BindingFlags.NonPublic, [])!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.ShoppingList!.Products, Is.Empty);
        }

        [Test]
        public async Task AddProductAsync_DelegatesToCoreOverload_WhenValid()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");

            var unitId = Guid.NewGuid();
            var unit = new UnitModel { Id = unitId, Name = "kg", UnitType = Common.Constants.Units.UnitType.Weight };
            var product = new ProductModel
            {
                Id = Guid.NewGuid(),
                Name = "Flour",
                BaseUnit = new UnitModel { Id = unitId, Name = "kg", UnitType = Common.Constants.Units.UnitType.Weight }
            };

            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [] };
            cut.Instance.Products = new PagedList<ProductModel>([product], new Metadata());
            cut.Instance.BaseUnits = new PagedList<UnitModel>([unit], new Metadata());
            cut.Instance.ProductId = product.Id.ToString();
            cut.Instance.UnitId = unitId.ToString();
            cut.Instance.Quantity = "2";

            var method = typeof(ShoppingListEdit).GetMethod("AddProductAsync", BindingFlags.Instance | BindingFlags.NonPublic, [])!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.ShoppingList!.Products, Has.Count.EqualTo(1));
            Assert.That(cut.Instance.ShoppingList!.Products!.Single().Product!.Id, Is.EqualTo(product.Id));
        }

        // ---------- AddProductAsync (3-arg core) ----------
        private static MethodInfo AddProductCoreMethod =>
            typeof(ShoppingListEdit).GetMethod(
                "AddProductAsync",
                BindingFlags.Instance | BindingFlags.NonPublic,
                [typeof(ProductModel), typeof(decimal), typeof(Guid)])!;

        [Test]
        public async Task AddProductAsyncCore_DoesNothing_WhenProductsListIsNull()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = null };
            cut.Instance.BaseUnits = new PagedList<UnitModel>([], new Metadata());

            var product = new ProductModel { Id = Guid.NewGuid(), Name = "Flour" };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)AddProductCoreMethod.Invoke(cut.Instance, [product, 1m, Guid.NewGuid()])!;
                await task;
            });

            // Assert — no exception, nothing to verify on a null Products list
            Assert.That(cut.Instance.ShoppingList!.Products, Is.Null);
        }

        [Test]
        public async Task AddProductAsyncCore_ShowsError_WhenUnitNotResolvable()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [] };
            cut.Instance.BaseUnits = new PagedList<UnitModel>([], new Metadata());

            var product = new ProductModel
            {
                Id = Guid.NewGuid(),
                Name = "Flour",
                BaseUnit = new UnitModel { Id = Guid.NewGuid(), Name = "kg", UnitType = Common.Constants.Units.UnitType.Weight }
            };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)AddProductCoreMethod.Invoke(cut.Instance, [product, 1m, Guid.NewGuid()])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Unit configuration is invalid.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
            Assert.That(cut.Instance.ShoppingList!.Products, Is.Empty);
        }

        [Test]
        public async Task AddProductAsyncCore_AccumulatesQuantity_ForExistingItem()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");

            var unitId = Guid.NewGuid();
            var unit = new UnitModel { Id = unitId, Name = "kg", UnitType = Common.Constants.Units.UnitType.Weight };
            var baseUnit = new UnitModel { Id = unitId, Name = "kg", UnitType = Common.Constants.Units.UnitType.Weight };
            var product = new ProductModel { Id = Guid.NewGuid(), Name = "Flour", BaseUnit = baseUnit };

            var existingItem = new ShoppingListProductEditModel { Product = product, Quantity = 1m, UnitId = unitId, Unit = unit, DisplaySequence = 1 };

            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [existingItem] };
            cut.Instance.BaseUnits = new PagedList<UnitModel>([unit], new Metadata());

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)AddProductCoreMethod.Invoke(cut.Instance, [product, 2m, unitId])!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.ShoppingList!.Products, Has.Count.EqualTo(1));
            Assert.That(cut.Instance.ShoppingList!.Products!.Single().Quantity, Is.EqualTo(3m));
        }

        [Test]
        public async Task AddProductAsyncCore_AddsNewItem_WithDisplaySequenceFromShop()
        {
            // Arrange
            ArrangeLookups();

            var shopId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var existingShoppingList = new ShoppingListEditModel { Id = Guid.NewGuid(), ShopId = shopId, Products = [] };
            var existingShop = new ShopEditModel
            {
                Id = shopId,
                DisplaySequence =
                [
                    new ShopDisplaySequenceEditModel(shopId, 7, new ProductCategoryModel { Id = categoryId, Name = "Cat" })
                ]
            };

            _shoppingListServiceMock
                .Setup(s => s.GetEditAsync(existingShoppingList.Id, CancellationToken.None))
                .ReturnsAsync(existingShoppingList);
            _shopServiceMock
                .Setup(s => s.GetEditAsync(shopId, CancellationToken.None))
                .ReturnsAsync(existingShop);

            var cut = RenderComponent(existingShoppingList.Id.ToString());

            var unitId = Guid.NewGuid();
            var baseUnit = new UnitModel { Id = unitId, Name = "kg", UnitType = Common.Constants.Units.UnitType.Weight };
            var product = new ProductModel
            {
                Id = Guid.NewGuid(),
                Name = "Flour",
                BaseUnit = baseUnit,
                ProductCategory = new ProductCategoryModel { Id = categoryId, Name = "Cat" }
            };

            cut.Instance.BaseUnits = new PagedList<UnitModel>([baseUnit], new Metadata());

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)AddProductCoreMethod.Invoke(cut.Instance, [product, 2m, unitId])!;
                await task;
            });

            // Assert
            var added = cut.Instance.ShoppingList!.Products!.Single();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(added.Product!.Id, Is.EqualTo(product.Id));
                Assert.That(added.DisplaySequence, Is.EqualTo(7));
                Assert.That(cut.Instance.Quantity, Is.EqualTo(string.Empty));
            }
        }

        [Test]
        public async Task AddProductAsyncCore_AddsNewItem_WithDefaultDisplaySequence_WhenShopIsNull()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [] };

            var unitId = Guid.NewGuid();
            var baseUnit = new UnitModel { Id = unitId, Name = "kg", UnitType = Common.Constants.Units.UnitType.Weight };
            var product = new ProductModel { Id = Guid.NewGuid(), Name = "Flour", BaseUnit = baseUnit };

            cut.Instance.BaseUnits = new PagedList<UnitModel>([baseUnit], new Metadata());

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)AddProductCoreMethod.Invoke(cut.Instance, [product, 2m, unitId])!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.ShoppingList!.Products!.Single().DisplaySequence, Is.EqualTo(1));
        }

        [Test]
        public async Task AddProductAsyncCore_ShowsExceptionMessage_WhenUnitConversionFails()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [] };

            var unitId = Guid.NewGuid();
            var unit = new UnitModel { Id = unitId, Name = "l", UnitType = Common.Constants.Units.UnitType.Liquid };
            var baseUnit = new UnitModel { Id = Guid.NewGuid(), Name = "kg", UnitType = Common.Constants.Units.UnitType.Weight };
            var product = new ProductModel { Id = Guid.NewGuid(), Name = "Flour", BaseUnit = baseUnit };

            cut.Instance.BaseUnits = new PagedList<UnitModel>([unit], new Metadata());

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)AddProductCoreMethod.Invoke(cut.Instance, [product, 2m, unitId])!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.ShoppingList!.Products, Is.Empty);
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }

        // ---------- CheckboxChangedAsync ----------
        [Test]
        public async Task CheckboxChangedAsync_DoesNothing_WhenItemNotFound()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [] };

            var model = new ShoppingListProductEditModel { Product = new ProductModel { Id = Guid.NewGuid() } };

            var method = typeof(ShoppingListEdit).GetMethod("CheckboxChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [model, true])!;
                await task;
            });

            // Assert
            _shoppingListServiceMock.Verify(
                s => s.UpdateProductCollectedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task CheckboxChangedAsync_RevertsValue_WhenUpdateFails()
        {
            // Arrange
            ArrangeLookups();
            var product = new ProductModel { Id = Guid.NewGuid(), Name = "Flour" };
            var item = new ShoppingListProductEditModel { Product = product, Collected = false, DisplaySequence = 1 };

            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [item] };

            _shoppingListServiceMock
                .Setup(s => s.UpdateProductCollectedAsync(cut.Instance.ShoppingList.Id, product.Id, true, CancellationToken.None))
                .ReturnsAsync(new CommandResponse { Succeeded = false, Message = "nope" });

            var method = typeof(ShoppingListEdit).GetMethod("CheckboxChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [new ShoppingListProductEditModel { Product = product }, true])!;
                await task;
            });

            // Assert
            Assert.That(item.Collected, Is.False);
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("nope", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task CheckboxChangedAsync_UpdatesAndResorts_WhenSucceeded()
        {
            // Arrange
            ArrangeLookups();
            var productA = new ProductModel { Id = Guid.NewGuid(), Name = "Bananas" };
            var productB = new ProductModel { Id = Guid.NewGuid(), Name = "Apples" };
            var itemA = new ShoppingListProductEditModel { Product = productA, Collected = false, DisplaySequence = 1 };
            var itemB = new ShoppingListProductEditModel { Product = productB, Collected = false, DisplaySequence = 2 };

            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [itemA, itemB] };

            _shoppingListServiceMock
                .Setup(s => s.UpdateProductCollectedAsync(cut.Instance.ShoppingList.Id, productA.Id, true, CancellationToken.None))
                .ReturnsAsync(new CommandResponse { Succeeded = true, Message = "ok" });

            var method = typeof(ShoppingListEdit).GetMethod("CheckboxChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [new ShoppingListProductEditModel { Product = productA }, true])!;
                await task;
            });

            // Assert
            Assert.That(itemA.Collected, Is.True);
            Assert.That(cut.Instance.ShoppingList!.Products!.Select(p => p.Product!.Name), Is.EqualTo(["Apples", "Bananas"]));
        }

        [Test]
        public async Task CheckboxChangedAsync_MiddleOfLargerList_MovesOnlyToggledItem_LeavesOthersInPlace()
        {
            // Arrange
            ArrangeLookups();
            var productA = new ProductModel { Id = Guid.NewGuid(), Name = "Apples" };
            var productB = new ProductModel { Id = Guid.NewGuid(), Name = "Bread" };
            var productC = new ProductModel { Id = Guid.NewGuid(), Name = "Cheese" };
            var productD = new ProductModel { Id = Guid.NewGuid(), Name = "Dates" };
            var itemA = new ShoppingListProductEditModel { Product = productA, Collected = false, DisplaySequence = 1 };
            var itemB = new ShoppingListProductEditModel { Product = productB, Collected = false, DisplaySequence = 2 };
            var itemC = new ShoppingListProductEditModel { Product = productC, Collected = false, DisplaySequence = 3 };
            var itemD = new ShoppingListProductEditModel { Product = productD, Collected = true, DisplaySequence = 1 };

            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [itemA, itemB, itemC, itemD] };

            _shoppingListServiceMock
                .Setup(s => s.UpdateProductCollectedAsync(cut.Instance.ShoppingList.Id, productC.Id, true, CancellationToken.None))
                .ReturnsAsync(new CommandResponse { Succeeded = true, Message = "ok" });

            var method = typeof(ShoppingListEdit).GetMethod("CheckboxChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [new ShoppingListProductEditModel { Product = productC }, true])!;
                await task;
            });

            // Assert
            Assert.That(itemC.Collected, Is.True);
            Assert.That(
                cut.Instance.ShoppingList!.Products!.Select(p => p.Product!.Name),
                Is.EqualTo(["Apples", "Bread", "Dates", "Cheese"]));
        }

        [Test]
        public async Task CheckboxChangedAsync_ItemAlreadyInCorrectPosition_DoesNotReorder()
        {
            // Arrange
            ArrangeLookups();
            var productA = new ProductModel { Id = Guid.NewGuid(), Name = "Apple" };
            var productB = new ProductModel { Id = Guid.NewGuid(), Name = "Banana" };
            var itemA = new ShoppingListProductEditModel { Product = productA, Collected = false, DisplaySequence = 1 };
            var itemB = new ShoppingListProductEditModel { Product = productB, Collected = true, DisplaySequence = 1 };

            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [itemA, itemB] };

            _shoppingListServiceMock
                .Setup(s => s.UpdateProductCollectedAsync(cut.Instance.ShoppingList.Id, productB.Id, false, CancellationToken.None))
                .ReturnsAsync(new CommandResponse { Succeeded = true, Message = "ok" });

            var method = typeof(ShoppingListEdit).GetMethod("CheckboxChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [new ShoppingListProductEditModel { Product = productB }, false])!;
                await task;
            });

            // Assert
            Assert.That(itemB.Collected, Is.False);
            Assert.That(
                cut.Instance.ShoppingList!.Products!.Select(p => p.Product!.Name),
                Is.EqualTo(["Apple", "Banana"]));
        }

        // ---------- OnProductChangedAsync ----------
        [Test]
        public async Task OnProductChangedAsync_ResetsQuantity_WhenProductIdBlank()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.Quantity = "5";

            var method = typeof(ShoppingListEdit).GetMethod("OnProductChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var args = new ChangeEventArgs { Value = "" };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [args])!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.Quantity, Is.EqualTo(string.Empty));
            _productServiceMock.Verify(s => s.GetEditAsync(It.IsAny<Guid>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task OnProductChangedAsync_ReturnsEarly_WhenProductNotFound()
        {
            // Arrange
            ArrangeLookups();
            var productId = Guid.NewGuid();

            _productServiceMock
                .Setup(s => s.GetEditAsync(productId, CancellationToken.None))
                .ReturnsAsync((ProductEditModel?)null);

            var cut = RenderComponent("0");
            var method = typeof(ShoppingListEdit).GetMethod("OnProductChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var args = new ChangeEventArgs { Value = productId.ToString() };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [args])!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.Units, Is.Null);
        }

        [Test]
        public async Task OnProductChangedAsync_ReturnsEarly_WhenBaseUnitNotFound()
        {
            // Arrange
            ArrangeLookups();
            var productId = Guid.NewGuid();
            var unknownBaseUnitId = Guid.NewGuid();

            _productServiceMock
                .Setup(s => s.GetEditAsync(productId, CancellationToken.None))
                .ReturnsAsync(new ProductEditModel { Id = productId, BaseUnitId = unknownBaseUnitId });

            var cut = RenderComponent("0");
            cut.Instance.BaseUnits = new PagedList<UnitModel>([], new Metadata());

            var method = typeof(ShoppingListEdit).GetMethod("OnProductChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var args = new ChangeEventArgs { Value = productId.ToString() };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [args])!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.Units, Is.Null);
        }

        [Test]
        public async Task OnProductChangedAsync_PopulatesUnits_OnHappyPath()
        {
            // Arrange
            ArrangeLookups();
            var productId = Guid.NewGuid();
            var baseUnitId = Guid.NewGuid();

            var baseUnit = new UnitModel { Id = baseUnitId, Name = "kg", UnitType = Common.Constants.Units.UnitType.Weight };
            var otherTypeUnit = new UnitModel { Id = Guid.NewGuid(), Name = "l", UnitType = Common.Constants.Units.UnitType.Liquid };

            _productServiceMock
                .Setup(s => s.GetEditAsync(productId, CancellationToken.None))
                .ReturnsAsync(new ProductEditModel { Id = productId, BaseUnitId = baseUnitId });

            var cut = RenderComponent("0");
            cut.Instance.BaseUnits = new PagedList<UnitModel>([baseUnit, otherTypeUnit], new Metadata());

            var method = typeof(ShoppingListEdit).GetMethod("OnProductChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var args = new ChangeEventArgs { Value = productId.ToString() };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [args])!;
                await task;
            });

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cut.Instance.Units!.Select(u => u.Id), Is.EquivalentTo(new[] { baseUnitId }));
                Assert.That(cut.Instance.UnitId, Is.EqualTo(baseUnitId.ToString()));
                Assert.That(cut.Instance.Units!.Single().IsSelected, Is.True);
            }
        }

        // ---------- OnShopChangedAsync ----------
        [Test]
        public async Task OnShopChangedAsync_ReturnsEarly_WhenGuidInvalid()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [] };

            var method = typeof(ShoppingListEdit).GetMethod("OnShopChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var args = new ChangeEventArgs { Value = "not-a-guid" };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [args])!;
                await task;
            });

            // Assert
            _shopServiceMock.Verify(s => s.GetEditAsync(It.IsAny<Guid>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task OnShopChangedAsync_ResortsProducts_ByDisplaySequenceFromShop()
        {
            // Arrange
            ArrangeLookups();

            var shopId = Guid.NewGuid();
            var categoryA = Guid.NewGuid();
            var categoryB = Guid.NewGuid();

            var productA = new ProductModel { Id = Guid.NewGuid(), Name = "Bananas", ProductCategory = new ProductCategoryModel { Id = categoryA, Name = "Fruit" } };
            var productB = new ProductModel { Id = Guid.NewGuid(), Name = "Bread", ProductCategory = new ProductCategoryModel { Id = categoryB, Name = "Bakery" } };

            var itemA = new ShoppingListProductEditModel { Product = productA, Collected = false, DisplaySequence = 1 };
            var itemB = new ShoppingListProductEditModel { Product = productB, Collected = false, DisplaySequence = 2 };

            var existingShop = new ShopEditModel
            {
                Id = shopId,
                DisplaySequence =
                [
                    new ShopDisplaySequenceEditModel(shopId, 10, new ProductCategoryModel { Id = categoryA, Name = "Fruit" }),
                    new ShopDisplaySequenceEditModel(shopId, 1, new ProductCategoryModel { Id = categoryB, Name = "Bakery" })
                ]
            };

            _shopServiceMock
                .Setup(s => s.GetEditAsync(shopId, CancellationToken.None))
                .ReturnsAsync(existingShop);

            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [itemA, itemB] };

            var method = typeof(ShoppingListEdit).GetMethod("OnShopChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var args = new ChangeEventArgs { Value = shopId.ToString() };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [args])!;
                await task;
            });

            // Assert — Bread (DisplaySequence 1) sorts before Bananas (DisplaySequence 10)
            Assert.That(cut.Instance.ShoppingList!.Products!.Select(p => p.Product!.Name), Is.EqualTo(["Bread", "Bananas"]));
        }

        // ---------- ConfirmDialog Yes/No (DeleteAsync / DeleteProductAsync) ----------
        private static void ClickConfirmDialogButton(IRenderedComponent<ShoppingListEdit> cut, string buttonText)
        {
            cut.WaitForAssertion(() => Assert.That(cut.FindAll(".modal-footer button"), Is.Not.Empty));
            var button = cut.FindAll(".modal-footer button").Single(b => b.TextContent.Trim() == buttonText);
            button.Click();
        }

        [Test]
        public async Task DeleteAsync_ConfirmedYes_DeletesShoppingList()
        {
            // Arrange
            ArrangeLookups();
            var id = Guid.NewGuid();
            var shopId = Guid.NewGuid();
            var existing = new ShoppingListEditModel { Id = id, ShopId = shopId, Products = [] };

            _shoppingListServiceMock.Setup(s => s.GetEditAsync(id, CancellationToken.None)).ReturnsAsync(existing);
            _shopServiceMock.Setup(s => s.GetEditAsync(shopId, CancellationToken.None)).ReturnsAsync(new ShopEditModel());
            _shoppingListServiceMock
                .Setup(s => s.DeleteAsync(id, CancellationToken.None))
                .ReturnsAsync(new CommandResponse { Succeeded = true, Message = "ok" });

            var cut = RenderComponent(id.ToString());
            var method = typeof(ShoppingListEdit).GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            var deleteTask = cut.InvokeAsync(() => (Task)method.Invoke(cut.Instance, [])!);
            ClickConfirmDialogButton(cut, "OK");
            await deleteTask;

            // Assert
            _shoppingListServiceMock.Verify(s => s.DeleteAsync(id, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_ConfirmedNo_DoesNotDelete()
        {
            // Arrange
            ArrangeLookups();
            var id = Guid.NewGuid();
            var shopId = Guid.NewGuid();
            var existing = new ShoppingListEditModel { Id = id, ShopId = shopId, Products = [] };

            _shoppingListServiceMock.Setup(s => s.GetEditAsync(id, CancellationToken.None)).ReturnsAsync(existing);
            _shopServiceMock.Setup(s => s.GetEditAsync(shopId, CancellationToken.None)).ReturnsAsync(new ShopEditModel());

            var cut = RenderComponent(id.ToString());
            var method = typeof(ShoppingListEdit).GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            var deleteTask = cut.InvokeAsync(() => (Task)method.Invoke(cut.Instance, [])!);
            ClickConfirmDialogButton(cut, "Cancel");
            await deleteTask;

            // Assert
            _shoppingListServiceMock.Verify(s => s.DeleteAsync(It.IsAny<Guid>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task DeleteProductAsync_ConfirmedYes_RemovesProduct()
        {
            // Arrange
            ArrangeLookups();
            var product = new ProductModel { Id = Guid.NewGuid(), Name = "Flour" };
            var item = new ShoppingListProductEditModel { Product = product, DisplaySequence = 1 };

            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [item] };

            var method = typeof(ShoppingListEdit).GetMethod("DeleteProductAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            var deleteTask = cut.InvokeAsync(() => (Task)method.Invoke(cut.Instance, [product])!);
            ClickConfirmDialogButton(cut, "OK");
            await deleteTask;

            // Assert
            Assert.That(cut.Instance.ShoppingList!.Products, Is.Empty);
        }

        [Test]
        public async Task DeleteProductAsync_ConfirmedNo_KeepsProduct()
        {
            // Arrange
            ArrangeLookups();
            var product = new ProductModel { Id = Guid.NewGuid(), Name = "Flour" };
            var item = new ShoppingListProductEditModel { Product = product, DisplaySequence = 1 };

            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [item] };

            var method = typeof(ShoppingListEdit).GetMethod("DeleteProductAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            var deleteTask = cut.InvokeAsync(() => (Task)method.Invoke(cut.Instance, [product])!);
            ClickConfirmDialogButton(cut, "Cancel");
            await deleteTask;

            // Assert
            Assert.That(cut.Instance.ShoppingList!.Products, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task DeleteProductAsync_DoesNothing_WhenProductNotInList()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [] };

            var method = typeof(ShoppingListEdit).GetMethod("DeleteProductAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [new ProductModel { Id = Guid.NewGuid() }])!;
                await task;
            });

            // Assert — no exception, list stays empty
            Assert.That(cut.Instance.ShoppingList!.Products, Is.Empty);
        }

        // ---------- CanAddMealPlan / AddMealPlanAsync ----------
        [Test]
        public async Task AddMealPlanAsync_ShowsError_WhenNoShopSelected()
        {
            // Arrange
            ArrangeLookups();
            var modalServiceMock = new Mock<IModalService>(MockBehavior.Strict);
            var cut = RenderComponent("0", modalServiceMock.Object);
            cut.Instance.ShoppingList = new ShoppingListEditModel { ShopId = Guid.Empty, Products = [] };

            var method = typeof(ShoppingListEdit).GetMethod("AddMealPlanAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("You must select a shop first.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
            modalServiceMock.Verify(m => m.Show<MealPlanSelection>(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task AddMealPlanAsync_ReturnsSilently_WhenModalServiceNotAvailable()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { ShopId = Guid.NewGuid(), Products = [] };

            var method = typeof(ShoppingListEdit).GetMethod("AddMealPlanAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _mealPlanServiceMock.Verify(
                s => s.GetShoppingListProductsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task AddMealPlanAsync_ShowsError_WhenModalNotConfirmed()
        {
            // Arrange
            ArrangeLookups();
            var modalReferenceMock = new Mock<IModalReference>(MockBehavior.Strict);
            modalReferenceMock.Setup(m => m.Result).Returns(Task.FromResult(ModalResult.Cancel()));

            var modalServiceMock = new Mock<IModalService>(MockBehavior.Strict);
            modalServiceMock.Setup(m => m.Show<MealPlanSelection>(It.IsAny<string>())).Returns(modalReferenceMock.Object);

            var cut = RenderComponent("0", modalServiceMock.Object);
            cut.Instance.ShoppingList = new ShoppingListEditModel { ShopId = Guid.NewGuid(), Products = [] };

            var method = typeof(ShoppingListEdit).GetMethod("AddMealPlanAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("You must select a meal plan to add to the shopping list.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
            _mealPlanServiceMock.Verify(
                s => s.GetShoppingListProductsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task AddMealPlanAsync_ShowsError_WhenConfirmedDataIsNotAGuid()
        {
            // Arrange
            ArrangeLookups();
            var modalReferenceMock = new Mock<IModalReference>(MockBehavior.Strict);
            modalReferenceMock.Setup(m => m.Result).Returns(Task.FromResult(ModalResult.Ok("not-a-guid")));

            var modalServiceMock = new Mock<IModalService>(MockBehavior.Strict);
            modalServiceMock.Setup(m => m.Show<MealPlanSelection>(It.IsAny<string>())).Returns(modalReferenceMock.Object);

            var cut = RenderComponent("0", modalServiceMock.Object);
            cut.Instance.ShoppingList = new ShoppingListEditModel { ShopId = Guid.NewGuid(), Products = [] };

            var method = typeof(ShoppingListEdit).GetMethod("AddMealPlanAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("You must select a meal plan to add to the shopping list.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task AddMealPlanAsync_AddsReturnedProducts_WhenConfirmed()
        {
            // Arrange
            ArrangeLookups();
            var mealPlanId = Guid.NewGuid();
            var shopId = Guid.NewGuid();

            var modalReferenceMock = new Mock<IModalReference>(MockBehavior.Strict);
            modalReferenceMock.Setup(m => m.Result).Returns(Task.FromResult(ModalResult.Ok(mealPlanId.ToString())));

            var modalServiceMock = new Mock<IModalService>(MockBehavior.Strict);
            modalServiceMock.Setup(m => m.Show<MealPlanSelection>(It.IsAny<string>())).Returns(modalReferenceMock.Object);

            var unitId = Guid.NewGuid();
            var baseUnit = new UnitModel { Id = unitId, Name = "kg", UnitType = Common.Constants.Units.UnitType.Weight };
            var product = new ProductModel { Id = Guid.NewGuid(), Name = "Flour", BaseUnit = baseUnit };

            _mealPlanServiceMock
                .Setup(s => s.GetShoppingListProductsAsync(mealPlanId, shopId, CancellationToken.None))
                .ReturnsAsync((IList<ShoppingListProductEditModel>?)
                    [new ShoppingListProductEditModel { Product = product, Quantity = 2m, UnitId = unitId }]);

            var cut = RenderComponent("0", modalServiceMock.Object);
            cut.Instance.ShoppingList = new ShoppingListEditModel { ShopId = shopId, Products = [] };
            cut.Instance.BaseUnits = new PagedList<UnitModel>([baseUnit], new Metadata());

            var method = typeof(ShoppingListEdit).GetMethod("AddMealPlanAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.ShoppingList!.Products, Has.Count.EqualTo(1));
            Assert.That(cut.Instance.ShoppingList!.Products!.Single().Product!.Id, Is.EqualTo(product.Id));
        }

        [Test]
        public async Task AddMealPlanAsync_ReturnsEarly_WhenProductsIsNull()
        {
            // Arrange
            ArrangeLookups();
            var mealPlanId = Guid.NewGuid();
            var shopId = Guid.NewGuid();

            var modalReferenceMock = new Mock<IModalReference>(MockBehavior.Strict);
            modalReferenceMock.Setup(m => m.Result).Returns(Task.FromResult(ModalResult.Ok(mealPlanId.ToString())));

            var modalServiceMock = new Mock<IModalService>(MockBehavior.Strict);
            modalServiceMock.Setup(m => m.Show<MealPlanSelection>(It.IsAny<string>())).Returns(modalReferenceMock.Object);

            _mealPlanServiceMock
                .Setup(s => s.GetShoppingListProductsAsync(mealPlanId, shopId, CancellationToken.None))
                .ReturnsAsync((IList<ShoppingListProductEditModel>?)null);

            var cut = RenderComponent("0", modalServiceMock.Object);
            cut.Instance.ShoppingList = new ShoppingListEditModel { ShopId = shopId, Products = [] };

            var method = typeof(ShoppingListEdit).GetMethod("AddMealPlanAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.ShoppingList!.Products, Is.Empty);
        }

        // ---------- CanAddRecipe / AddRecipeAsync ----------
        [Test]
        public async Task AddRecipeAsync_ShowsError_WhenNoShopSelected()
        {
            // Arrange
            ArrangeLookups();
            var modalServiceMock = new Mock<IModalService>(MockBehavior.Strict);
            var cut = RenderComponent("0", modalServiceMock.Object);
            cut.Instance.ShoppingList = new ShoppingListEditModel { ShopId = Guid.Empty, Products = [] };

            var method = typeof(ShoppingListEdit).GetMethod("AddRecipeAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("You must select a shop first.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
            modalServiceMock.Verify(m => m.Show<RecipeSelection>(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task AddRecipeAsync_ReturnsSilently_WhenModalServiceNotAvailable()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { ShopId = Guid.NewGuid(), Products = [] };

            var method = typeof(ShoppingListEdit).GetMethod("AddRecipeAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _recipeServiceMock.Verify(
                s => s.GetShoppingListProductsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task AddRecipeAsync_ShowsError_WhenModalNotConfirmed()
        {
            // Arrange
            ArrangeLookups();
            var modalReferenceMock = new Mock<IModalReference>(MockBehavior.Strict);
            modalReferenceMock.Setup(m => m.Result).Returns(Task.FromResult(ModalResult.Cancel()));

            var modalServiceMock = new Mock<IModalService>(MockBehavior.Strict);
            modalServiceMock.Setup(m => m.Show<RecipeSelection>(It.IsAny<string>())).Returns(modalReferenceMock.Object);

            var cut = RenderComponent("0", modalServiceMock.Object);
            cut.Instance.ShoppingList = new ShoppingListEditModel { ShopId = Guid.NewGuid(), Products = [] };

            var method = typeof(ShoppingListEdit).GetMethod("AddRecipeAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("You must select a recipe to add to the shopping list.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
            _recipeServiceMock.Verify(
                s => s.GetShoppingListProductsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task AddRecipeAsync_AddsReturnedProducts_WhenConfirmed()
        {
            // Arrange
            ArrangeLookups();
            var recipeId = Guid.NewGuid();
            var shopId = Guid.NewGuid();

            var modalReferenceMock = new Mock<IModalReference>(MockBehavior.Strict);
            modalReferenceMock.Setup(m => m.Result).Returns(Task.FromResult(ModalResult.Ok(recipeId.ToString())));

            var modalServiceMock = new Mock<IModalService>(MockBehavior.Strict);
            modalServiceMock.Setup(m => m.Show<RecipeSelection>(It.IsAny<string>())).Returns(modalReferenceMock.Object);

            var unitId = Guid.NewGuid();
            var baseUnit = new UnitModel { Id = unitId, Name = "kg", UnitType = Common.Constants.Units.UnitType.Weight };
            var product = new ProductModel { Id = Guid.NewGuid(), Name = "Sugar", BaseUnit = baseUnit };

            _recipeServiceMock
                .Setup(s => s.GetShoppingListProductsAsync(recipeId, shopId, CancellationToken.None))
                .ReturnsAsync((IList<ShoppingListProductEditModel>?)
                    [new ShoppingListProductEditModel { Product = product, Quantity = 1m, UnitId = unitId }]);

            var cut = RenderComponent("0", modalServiceMock.Object);
            cut.Instance.ShoppingList = new ShoppingListEditModel { ShopId = shopId, Products = [] };
            cut.Instance.BaseUnits = new PagedList<UnitModel>([baseUnit], new Metadata());

            var method = typeof(ShoppingListEdit).GetMethod("AddRecipeAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.ShoppingList!.Products, Has.Count.EqualTo(1));
            Assert.That(cut.Instance.ShoppingList!.Products!.Single().Product!.Id, Is.EqualTo(product.Id));
        }

        [Test]
        public async Task AddRecipeAsync_ReturnsEarly_WhenProductsIsNull()
        {
            // Arrange
            ArrangeLookups();
            var recipeId = Guid.NewGuid();
            var shopId = Guid.NewGuid();

            var modalReferenceMock = new Mock<IModalReference>(MockBehavior.Strict);
            modalReferenceMock.Setup(m => m.Result).Returns(Task.FromResult(ModalResult.Ok(recipeId.ToString())));

            var modalServiceMock = new Mock<IModalService>(MockBehavior.Strict);
            modalServiceMock.Setup(m => m.Show<RecipeSelection>(It.IsAny<string>())).Returns(modalReferenceMock.Object);

            _recipeServiceMock
                .Setup(s => s.GetShoppingListProductsAsync(recipeId, shopId, CancellationToken.None))
                .ReturnsAsync((IList<ShoppingListProductEditModel>?)null);

            var cut = RenderComponent("0", modalServiceMock.Object);
            cut.Instance.ShoppingList = new ShoppingListEditModel { ShopId = shopId, Products = [] };

            var method = typeof(ShoppingListEdit).GetMethod("AddRecipeAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.ShoppingList!.Products, Is.Empty);
        }

        // ---------- Export ----------
        [Test]
        public async Task Export_CopiesEmptyString_WhenNoProducts()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [] };

            _ctx.JSInterop.SetupVoid("copyTextToClipboard", _ => true).SetVoidResult();

            var method = typeof(ShoppingListEdit).GetMethod("Export", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            var invocation = _ctx.JSInterop.Invocations["copyTextToClipboard"].Single();
            Assert.That(invocation.Arguments[0], Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task Export_CopiesFormattedText_WhenProductsPresent_AndResetsCopiedMessage()
        {
            // Arrange
            ArrangeLookups();
            var product = new ProductModel { Id = Guid.NewGuid(), Name = "Flour" };
            var unit = new UnitModel { Id = Guid.NewGuid(), Name = "kg" };
            var item = new ShoppingListProductEditModel { Product = product, Unit = unit, Quantity = 2m, DisplaySequence = 1 };

            var cut = RenderComponent("0");
            cut.Instance.ShoppingList = new ShoppingListEditModel { Products = [item] };

            _ctx.JSInterop.SetupVoid("copyTextToClipboard", _ => true).SetVoidResult();

            var method = typeof(ShoppingListEdit).GetMethod("Export", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            var invocation = _ctx.JSInterop.Invocations["copyTextToClipboard"].Single();
            Assert.That(invocation.Arguments[0], Is.EqualTo("Flour - 2 kg"));
        }
    }
}