using System.Reflection;
using Bunit;
using Common.Constants.Units;
using Common.Models;
using Common.Pagination;
using Common.UI;
using MealPlanner.UI.Web.Pages.RecipeBooks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Tests.Pages.RecipeBooks
{
    [TestFixture]
    public class RecipeEditTests
    {
        private BunitContext _ctx = null!;
        private Mock<IRecipeService> _recipeServiceMock = null!;
        private Mock<IRecipeCategoryService> _recipeCategoryServiceMock = null!;
        private Mock<IProductCategoryService> _productCategoryServiceMock = null!;
        private Mock<IProductService> _productServiceMock = null!;
        private Mock<IUnitService> _unitServiceMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _recipeServiceMock = new Mock<IRecipeService>(MockBehavior.Strict);
            _recipeCategoryServiceMock = new Mock<IRecipeCategoryService>(MockBehavior.Strict);
            _productCategoryServiceMock = new Mock<IProductCategoryService>(MockBehavior.Strict);
            _productServiceMock = new Mock<IProductService>(MockBehavior.Strict);
            _unitServiceMock = new Mock<IUnitService>(MockBehavior.Strict);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_recipeServiceMock.Object);
            _ctx.Services.AddSingleton(_recipeCategoryServiceMock.Object);
            _ctx.Services.AddSingleton(_productCategoryServiceMock.Object);
            _ctx.Services.AddSingleton(_productServiceMock.Object);
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

        private void ArrangeLookups()
        {
            _recipeCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([], new Metadata()));

            _productCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], new Metadata()));

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<UnitModel>([], new Metadata()));
        }

        private IRenderedComponent<RecipeEdit> RenderComponent(string? id = null)
        {
            return _ctx.Render<RecipeEdit>(ps =>
            {
                if (id is not null)
                    ps.Add(p => p.Id, id);

                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
            });
        }

        private static void ClickConfirmDialogButton(IRenderedComponent<RecipeEdit> cut, bool confirm)
        {
            // RecipeEdit sets YesButtonColor = Success and NoButtonColor = Danger on its ConfirmDialogOptions.
            var selector = confirm ? "button.btn-success.px-4" : "button.btn-danger.me-md-2.px-4";
            cut.WaitForElement(selector).Click();
        }

        // ---------- OnInitializedAsync ----------
        [Test]
        public void OnInitializedAsync_WithIdZero_CreatesNewRecipe()
        {
            // Arrange
            ArrangeLookups();

            // Act
            var cut = RenderComponent(Guid.Empty.ToString());

            // Assert
            Assert.That(cut.Instance.Recipe, Is.Not.Null);
            Assert.That(cut.Instance.Recipe!.Id, Is.EqualTo(Guid.Empty));

            _recipeServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public void OnInitializedAsync_WithNonZeroId_LoadsRecipe()
        {
            // Arrange
            ArrangeLookups();

            var recipeId = Guid.NewGuid();
            var existing = new RecipeEditModel { Id = recipeId, Name = "Loaded" };

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(recipeId, CancellationToken.None))
                .ReturnsAsync(existing);

            // Act
            var cut = RenderComponent(recipeId.ToString());

            // Assert
            Assert.That(cut.Instance.Recipe, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cut.Instance.Recipe!.Id, Is.EqualTo(recipeId));
                Assert.That(cut.Instance.Recipe!.Name, Is.EqualTo("Loaded"));
            }

            _recipeServiceMock.Verify(s => s.GetEditAsync(recipeId, CancellationToken.None), Times.Once);
        }

        [Test]
        public void OnInitializedAsync_WithNonZeroId_NullFromService_FallsBackToRecipeWithId()
        {
            // Arrange
            ArrangeLookups();

            var recipeId = Guid.NewGuid();

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(recipeId, CancellationToken.None))
                .ReturnsAsync((RecipeEditModel?)null);

            // Act
            var cut = RenderComponent(recipeId.ToString());

            // Assert
            Assert.That(cut.Instance.Recipe, Is.Not.Null);
            Assert.That(cut.Instance.Recipe!.Id, Is.EqualTo(recipeId));
            _recipeServiceMock.Verify(s => s.GetEditAsync(recipeId, CancellationToken.None), Times.Once);
        }

        // ---------- SaveCoreAsync ----------
        [Test]
        public async Task SaveCoreAsync_AddsRecipe_WhenIdIsZero()
        {
            // Arrange
            ArrangeLookups();

            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            _recipeServiceMock
                .Setup(s => s.AddAsync(It.IsAny<RecipeEditModel>(), CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent("0");

            var recipe = new RecipeEditModel { Id = Guid.Empty, Name = "New Recipe" };

            var method = typeof(RecipeEdit)
                .GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [recipe])!;
                await task;
            });

            // Assert
            _recipeServiceMock.Verify(
                s => s.AddAsync(It.Is<RecipeEditModel>(r => r.Name == "New Recipe"), CancellationToken.None),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Data has been saved successfully", It.IsAny<string>(), CancellationToken.None),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/recipesoverview"));
        }

        [Test]
        public async Task SaveCoreAsync_UpdatesRecipe_WhenIdIsNonZero()
        {
            // Arrange
            ArrangeLookups();

            var recipeId = Guid.NewGuid();
            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            var existing = new RecipeEditModel { Id = recipeId, Name = "Loaded" };

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(recipeId, CancellationToken.None))
                .ReturnsAsync(existing);

            _recipeServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<RecipeEditModel>(), CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent(recipeId.ToString());

            var recipe = new RecipeEditModel { Id = recipeId, Name = "Updated" };

            var method = typeof(RecipeEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [recipe])!;
                await task;
            });

            // Assert
            _recipeServiceMock.Verify(s => s.GetEditAsync(recipeId, CancellationToken.None), Times.Once);
            _recipeServiceMock.Verify(
                s => s.UpdateAsync(It.Is<RecipeEditModel>(r => r.Id == recipeId), CancellationToken.None),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Data has been saved successfully", It.IsAny<string>(), CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_ShowsGenericError_WhenResponseIsNull()
        {
            // Arrange
            ArrangeLookups();

            _recipeServiceMock
                .Setup(s => s.AddAsync(It.IsAny<RecipeEditModel>(), CancellationToken.None))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent("0");

            var recipe = new RecipeEditModel { Id = Guid.Empty, Name = "New Recipe" };

            var method = typeof(RecipeEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [recipe])!;
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

            _recipeServiceMock
                .Setup(s => s.AddAsync(It.IsAny<RecipeEditModel>(), CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent("0");

            var recipe = new RecipeEditModel { Id = Guid.Empty, Name = "New Recipe" };

            var method = typeof(RecipeEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [recipe])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Validation error", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }

        // ---------- DeleteAsync / DeleteCoreAsync ----------
        [Test]
        public async Task DeleteAsync_DoesNothing_WhenRecipeIsNullOrIdZero()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.Recipe = new RecipeEditModel { Id = Guid.Empty };

            var method = typeof(RecipeEdit).GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _recipeServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task DeleteCoreAsync_Deletes_WhenResponseSucceeded()
        {
            // Arrange
            ArrangeLookups();

            var recipeId = Guid.NewGuid();
            var response = new CommandResponse
            {
                Succeeded = true,
                Message = "ok"
            };

            var existing = new RecipeEditModel { Id = recipeId, Name = "Loaded" };

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(recipeId, CancellationToken.None))
                .ReturnsAsync(existing);

            _recipeServiceMock
                .Setup(s => s.DeleteAsync(recipeId, CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent(recipeId.ToString());

            var recipe = new RecipeEditModel { Id = recipeId, Name = "ToDelete" };

            var method = typeof(RecipeEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [recipe])!;
                await task;
            });

            // Assert
            _recipeServiceMock.Verify(s => s.GetEditAsync(recipeId, CancellationToken.None), Times.Once);
            _recipeServiceMock.Verify(s => s.DeleteAsync(recipeId, CancellationToken.None), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Data has been deleted successfully", It.IsAny<string>(), CancellationToken.None),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/recipesoverview"));
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            ArrangeLookups();

            var recipeId = Guid.NewGuid();
            var existing = new RecipeEditModel { Id = recipeId, Name = "Loaded" };

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(recipeId, CancellationToken.None))
                .ReturnsAsync(existing);

            _recipeServiceMock
                .Setup(s => s.DeleteAsync(recipeId, CancellationToken.None))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent(recipeId.ToString());

            var recipe = new RecipeEditModel { Id = recipeId };

            var method = typeof(RecipeEdit)
                .GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [recipe])!;
                await task;
            });

            // Assert
            _recipeServiceMock.Verify(s => s.GetEditAsync(recipeId, CancellationToken.None), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Delete failed. Please try again.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsResponseMessage_WhenFailed()
        {
            // Arrange
            ArrangeLookups();

            var recipeId = Guid.NewGuid();
            var response = new CommandResponse
            {
                Succeeded = false,
                Message = "Delete failed because of dependency"
            };

            var existing = new RecipeEditModel { Id = recipeId, Name = "Loaded" };

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(recipeId, CancellationToken.None))
                .ReturnsAsync(existing);

            _recipeServiceMock
                .Setup(s => s.DeleteAsync(recipeId, CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent(recipeId.ToString());

            var recipe = new RecipeEditModel { Id = recipeId };

            var method = typeof(RecipeEdit)
                .GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [recipe])!;
                await task;
            });

            // Assert
            _recipeServiceMock.Verify(s => s.GetEditAsync(recipeId, CancellationToken.None), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Delete failed because of dependency", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }

        // ---------- NavigateToOverview ----------
        [Test]
        public void NavigateToOverview_NavigatesToOverviewUrl()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            var method = typeof(RecipeEdit)
                .GetMethod("NavigateToOverview", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            cut.InvokeAsync(() => method!.Invoke(cut.Instance, []));

            // Assert
            Assert.That(nav.Uri, Does.EndWith("recipebooks/recipesoverview"));
        }

        // ---------- CanAddIngredient ----------
        [Test]
        public void CanAddIngredient_False_WhenProductIdMissing()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ProductId = "";
            cut.Instance.UnitId = Guid.NewGuid().ToString();
            cut.Instance.Quantity = "2";

            var property = typeof(RecipeEdit).GetProperty("CanAddIngredient", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null);
            Assert.That((bool)property!.GetValue(cut.Instance)!, Is.False);
        }

        [Test]
        public void CanAddIngredient_False_WhenProductIdIsZero()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ProductId = "0";
            cut.Instance.UnitId = Guid.NewGuid().ToString();
            cut.Instance.Quantity = "2";

            var property = typeof(RecipeEdit).GetProperty("CanAddIngredient", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That((bool)property!.GetValue(cut.Instance)!, Is.False);
        }

        [Test]
        public void CanAddIngredient_False_WhenUnitIdIsZero()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ProductId = Guid.NewGuid().ToString();
            cut.Instance.UnitId = "0";
            cut.Instance.Quantity = "2";

            var property = typeof(RecipeEdit).GetProperty("CanAddIngredient", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That((bool)property!.GetValue(cut.Instance)!, Is.False);
        }

        [Test]
        public void CanAddIngredient_False_WhenQuantityNotNumeric()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ProductId = Guid.NewGuid().ToString();
            cut.Instance.UnitId = Guid.NewGuid().ToString();
            cut.Instance.Quantity = "abc";

            var property = typeof(RecipeEdit).GetProperty("CanAddIngredient", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That((bool)property!.GetValue(cut.Instance)!, Is.False);
        }

        [Test]
        public void CanAddIngredient_False_WhenQuantityIsZeroOrNegative()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ProductId = Guid.NewGuid().ToString();
            cut.Instance.UnitId = Guid.NewGuid().ToString();
            cut.Instance.Quantity = "-1";

            var property = typeof(RecipeEdit).GetProperty("CanAddIngredient", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That((bool)property!.GetValue(cut.Instance)!, Is.False);
        }

        [Test]
        public void CanAddIngredient_True_WhenAllValid()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ProductId = Guid.NewGuid().ToString();
            cut.Instance.UnitId = Guid.NewGuid().ToString();
            cut.Instance.Quantity = "2.5";

            var property = typeof(RecipeEdit).GetProperty("CanAddIngredient", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That((bool)property!.GetValue(cut.Instance)!, Is.True);
        }

        // ---------- AddIngredientAsync ----------
        [Test]
        public async Task AddIngredientAsync_DoesNothing_WhenRecipeIsNull()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.Recipe = null;
            cut.Instance.ProductId = Guid.NewGuid().ToString();

            var method = typeof(RecipeEdit).GetMethod("AddIngredientAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            Assert.That(cut.Instance.Recipe, Is.Null);
        }

        [Test]
        public async Task AddIngredientAsync_DoesNothing_WhenProductIdMissing()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.Recipe = new RecipeEditModel { Id = Guid.Empty };
            cut.Instance.ProductId = "0";

            var method = typeof(RecipeEdit).GetMethod("AddIngredientAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            Assert.That(cut.Instance.Recipe!.Ingredients, Is.Empty);
        }

        [Test]
        public async Task AddIngredientAsync_AddsNewIngredient_WhenIngredientsListIsNull()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            var productId = Guid.NewGuid();
            var unitId = Guid.NewGuid();

            cut.Instance.Recipe = new RecipeEditModel { Id = Guid.Empty, Ingredients = null };
            cut.Instance.ProductId = productId.ToString();
            cut.Instance.UnitId = unitId.ToString();
            cut.Instance.Quantity = "3";

            var method = typeof(RecipeEdit).GetMethod("AddIngredientAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            Assert.That(cut.Instance.Recipe!.Ingredients, Has.Count.EqualTo(1));
            var added = cut.Instance.Recipe.Ingredients![0];
            using (Assert.EnterMultipleScope())
            {
                Assert.That(added.ProductId, Is.EqualTo(productId));
                Assert.That(added.UnitId, Is.EqualTo(unitId));
                Assert.That(added.Quantity, Is.EqualTo(3));
            }
            Assert.That(cut.Instance.Quantity, Is.EqualTo(string.Empty));
            Assert.That(cut.Instance.UnitId, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task AddIngredientAsync_AccumulatesQuantity_WhenSameProductAndUnitExists()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            var productId = Guid.NewGuid();
            var unitId = Guid.NewGuid();
            var product = new ProductModel { Id = productId, Name = "Flour" };
            var unit = new UnitModel { Id = unitId, Name = "kg" };

            var existing = new RecipeIngredientEditModel
            {
                ProductId = productId,
                Product = product,
                UnitId = unitId,
                Unit = unit,
                Quantity = 2
            };

            cut.Instance.Recipe = new RecipeEditModel { Id = Guid.Empty, Ingredients = [existing] };
            cut.Instance.ProductId = productId.ToString();
            cut.Instance.UnitId = unitId.ToString();
            cut.Instance.Quantity = "3";

            var method = typeof(RecipeEdit).GetMethod("AddIngredientAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            Assert.That(cut.Instance.Recipe!.Ingredients, Has.Count.EqualTo(1));
            Assert.That(existing.Quantity, Is.EqualTo(5));
        }

        [Test]
        public async Task AddIngredientAsync_ShowsError_WhenSameProductDifferentUnitExists()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            var productId = Guid.NewGuid();
            var existingUnitId = Guid.NewGuid();
            var newUnitId = Guid.NewGuid();
            var product = new ProductModel { Id = productId, Name = "Flour" };

            var existing = new RecipeIngredientEditModel
            {
                ProductId = productId,
                Product = product,
                UnitId = existingUnitId,
                Unit = new UnitModel { Id = existingUnitId, Name = "kg" },
                Quantity = 2
            };

            cut.Instance.Recipe = new RecipeEditModel { Id = Guid.Empty, Ingredients = [existing] };
            cut.Instance.ProductId = productId.ToString();
            cut.Instance.UnitId = newUnitId.ToString();
            cut.Instance.Quantity = "3";

            var method = typeof(RecipeEdit).GetMethod("AddIngredientAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            Assert.That(existing.Quantity, Is.EqualTo(2));
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync(
                    "The same ingredient was added to the recipe with a different unit of measurement.",
                    It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }

        // ---------- DeleteIngredientAsync ----------
        [Test]
        public async Task DeleteIngredientAsync_DoesNothing_WhenIngredientsIsNull()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.Recipe = new RecipeEditModel { Id = Guid.NewGuid(), Ingredients = null };
            var product = new ProductModel { Id = Guid.NewGuid(), Name = "Flour" };

            var method = typeof(RecipeEdit).GetMethod("DeleteIngredientAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [product])!;
                await task;
            });

            Assert.That(cut.Instance.Recipe!.Ingredients, Is.Null);
        }

        [Test]
        public async Task DeleteIngredientAsync_DoesNothing_WhenNoMatchingIngredient()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            var existingProduct = new ProductModel { Id = Guid.NewGuid(), Name = "Flour" };
            var ingredient = new RecipeIngredientEditModel { Product = existingProduct, ProductId = existingProduct.Id };
            cut.Instance.Recipe = new RecipeEditModel { Id = Guid.NewGuid(), Ingredients = [ingredient] };

            var otherProduct = new ProductModel { Id = Guid.NewGuid(), Name = "Sugar" };

            var method = typeof(RecipeEdit).GetMethod("DeleteIngredientAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [otherProduct])!;
                await task;
            });

            Assert.That(cut.Instance.Recipe!.Ingredients, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task DeleteIngredientAsync_KeepsIngredient_WhenConfirmationDeclined()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            var product = new ProductModel { Id = Guid.NewGuid(), Name = "Flour" };
            var ingredient = new RecipeIngredientEditModel { Product = product, ProductId = product.Id };
            cut.Instance.Recipe = new RecipeEditModel { Id = Guid.NewGuid(), Ingredients = [ingredient] };

            var method = typeof(RecipeEdit).GetMethod("DeleteIngredientAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            Task workTask = null!;
            await cut.InvokeAsync(() => { workTask = (Task)method!.Invoke(cut.Instance, [product])!; });

            ClickConfirmDialogButton(cut, confirm: false);
            await workTask;

            Assert.That(cut.Instance.Recipe!.Ingredients, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task DeleteIngredientAsync_RemovesIngredient_WhenConfirmationAccepted()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            var product = new ProductModel { Id = Guid.NewGuid(), Name = "Flour" };
            var ingredient = new RecipeIngredientEditModel { Product = product, ProductId = product.Id };
            cut.Instance.Recipe = new RecipeEditModel { Id = Guid.NewGuid(), Ingredients = [ingredient] };

            var method = typeof(RecipeEdit).GetMethod("DeleteIngredientAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            Task workTask = null!;
            await cut.InvokeAsync(() => { workTask = (Task)method!.Invoke(cut.Instance, [product])!; });

            ClickConfirmDialogButton(cut, confirm: true);
            await workTask;

            Assert.That(cut.Instance.Recipe!.Ingredients, Is.Empty);
        }

        // ---------- OnProductCategoryChangedAsync ----------
        [Test]
        public async Task OnProductCategoryChangedAsync_SearchesWithFilter_WhenCategorySelected()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.ProductId = Guid.NewGuid().ToString();
            cut.Instance.Quantity = "5";

            var categoryId = Guid.NewGuid().ToString();
            var products = new PagedList<ProductModel>([new ProductModel { Id = Guid.NewGuid(), Name = "Milk" }], new Metadata());

            _productServiceMock
                .Setup(s => s.SearchAsync(
                    It.Is<QueryParameters<ProductModel>>(q => q.Filters!.Any(f => f.PropertyName == "ProductCategoryId" && (string)f.Value! == categoryId)),
                    CancellationToken.None))
                .ReturnsAsync(products);

            var method = typeof(RecipeEdit).GetMethod("OnProductCategoryChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var args = new ChangeEventArgs { Value = categoryId };

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [args])!;
                await task;
            });

            Assert.That(cut.Instance.Products, Is.SameAs(products));
            Assert.That(cut.Instance.ProductId, Is.EqualTo(string.Empty));
            Assert.That(cut.Instance.Quantity, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task OnProductCategoryChangedAsync_SearchesWithoutFilter_WhenCategoryCleared()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");

            var products = new PagedList<ProductModel>([], new Metadata());

            _productServiceMock
                .Setup(s => s.SearchAsync(
                    It.Is<QueryParameters<ProductModel>>(q => q.Filters != null && !q.Filters.Any()),
                    CancellationToken.None))
                .ReturnsAsync(products);

            var method = typeof(RecipeEdit).GetMethod("OnProductCategoryChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var args = new ChangeEventArgs { Value = "" };

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [args])!;
                await task;
            });

            _productServiceMock.Verify(
                s => s.SearchAsync(It.Is<QueryParameters<ProductModel>>(q => q.Filters != null && !q.Filters.Any()), CancellationToken.None),
                Times.Once);
        }

        // ---------- OnProductChangedAsync ----------
        [Test]
        public async Task OnProductChangedAsync_ResetsQuantity_WhenProductIdEmpty()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.Quantity = "5";

            var method = typeof(RecipeEdit).GetMethod("OnProductChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var args = new ChangeEventArgs { Value = "" };

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [args])!;
                await task;
            });

            Assert.That(cut.Instance.Quantity, Is.EqualTo(string.Empty));
            _productServiceMock.Verify(s => s.GetEditAsync(It.IsAny<Guid>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task OnProductChangedAsync_DoesNothing_WhenProductNotFound()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            var productId = Guid.NewGuid();

            _productServiceMock
                .Setup(s => s.GetEditAsync(productId, CancellationToken.None))
                .ReturnsAsync((ProductEditModel?)null);

            var method = typeof(RecipeEdit).GetMethod("OnProductChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var args = new ChangeEventArgs { Value = productId.ToString() };

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [args])!;
                await task;
            });

            Assert.That(cut.Instance.Units, Is.Null);
        }

        [Test]
        public async Task OnProductChangedAsync_DoesNothing_WhenBaseUnitsMissing()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            var productId = Guid.NewGuid();

            _productServiceMock
                .Setup(s => s.GetEditAsync(productId, CancellationToken.None))
                .ReturnsAsync(new ProductEditModel { Id = productId, BaseUnitId = Guid.NewGuid() });

            cut.Instance.BaseUnits = null;

            var method = typeof(RecipeEdit).GetMethod("OnProductChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var args = new ChangeEventArgs { Value = productId.ToString() };

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [args])!;
                await task;
            });

            Assert.That(cut.Instance.Units, Is.Null);
        }

        [Test]
        public async Task OnProductChangedAsync_DoesNothing_WhenBaseUnitNotInList()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            var productId = Guid.NewGuid();
            var baseUnitId = Guid.NewGuid();

            _productServiceMock
                .Setup(s => s.GetEditAsync(productId, CancellationToken.None))
                .ReturnsAsync(new ProductEditModel { Id = productId, BaseUnitId = baseUnitId });

            cut.Instance.BaseUnits = new PagedList<UnitModel>(
                [new UnitModel { Id = Guid.NewGuid(), Name = "Other", UnitType = UnitType.Piece }],
                new Metadata());

            var method = typeof(RecipeEdit).GetMethod("OnProductChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var args = new ChangeEventArgs { Value = productId.ToString() };

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [args])!;
                await task;
            });

            Assert.That(cut.Instance.Units, Is.Null);
        }

        [Test]
        public async Task OnProductChangedAsync_PopulatesUnits_WhenBaseUnitFound()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            var productId = Guid.NewGuid();
            var baseUnitId = Guid.NewGuid();
            var otherWeightUnitId = Guid.NewGuid();
            var volumeUnitId = Guid.NewGuid();

            _productServiceMock
                .Setup(s => s.GetEditAsync(productId, CancellationToken.None))
                .ReturnsAsync(new ProductEditModel { Id = productId, BaseUnitId = baseUnitId });

            cut.Instance.BaseUnits = new PagedList<UnitModel>(
            [
                new UnitModel { Id = baseUnitId, Name = "Kilogram", UnitType = UnitType.Weight },
                new UnitModel { Id = otherWeightUnitId, Name = "Gram", UnitType = UnitType.Weight },
                new UnitModel { Id = volumeUnitId, Name = "Liter", UnitType = UnitType.Volume }
            ], new Metadata());

            var method = typeof(RecipeEdit).GetMethod("OnProductChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var args = new ChangeEventArgs { Value = productId.ToString() };

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [args])!;
                await task;
            });

            Assert.That(cut.Instance.Units, Has.Count.EqualTo(2));
            Assert.That(cut.Instance.UnitId, Is.EqualTo(baseUnitId.ToString()));
            var selected = cut.Instance.Units!.First(u => u.Id == baseUnitId);
            Assert.That(selected.IsSelected, Is.True);
        }

        // ---------- OnInputFileChangeAsync ----------
        [Test]
        public async Task OnInputFileChangeAsync_SetsImageContent_WhenWithinLimit()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.Recipe = new RecipeEditModel { Id = Guid.NewGuid() };

            var bytes = new byte[] { 1, 2, 3, 4 };
            var file = new FakeBrowserFile(bytes, "img.png", "image/png");
            var args = new InputFileChangeEventArgs([file]);

            var method = typeof(RecipeEdit).GetMethod("OnInputFileChangeAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [args])!;
                await task;
            });

            Assert.That(cut.Instance.Recipe.ImageContent, Is.Not.Null);
            Assert.That(cut.Instance.Recipe.ImageContent!.SequenceEqual(bytes), Is.True);
        }

        [Test]
        public async Task OnInputFileChangeAsync_ShowsError_WhenFileTooLarge()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.Recipe = new RecipeEditModel { Id = Guid.NewGuid() };

            var bigBytes = new byte[1024 * 1024 * 5];
            var file = new FakeBrowserFile(bigBytes, "big.bin", "application/octet-stream", throwOnOpen: true);
            var args = new InputFileChangeEventArgs([file]);

            var method = typeof(RecipeEdit).GetMethod("OnInputFileChangeAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [args])!;
                await task;
            });

            _messageComponentMock.Verify(
                m => m.ShowErrorAsync(
                    "File size exceeds the limit. Maximum allowed size is <strong>3 MB</strong>.",
                    It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }

        // ---------- CheckQuantityAsync ----------
        [Test]
        public async Task CheckQuantityAsync_InvokesJavaScript()
        {
            ArrangeLookups();
            _ctx.JSInterop.SetupVoid("checkQuantity", _ => true).SetVoidResult();
            var cut = RenderComponent("0");

            var method = typeof(RecipeEdit).GetMethod("CheckQuantityAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [new ChangeEventArgs()])!;
                await task;
            });

            Assert.That(_ctx.JSInterop.Invocations.Any(i => i.Identifier == "checkQuantity"), Is.True);
        }

        // ---------- SaveAsync ----------
        [Test]
        public async Task SaveAsync_DoesNothing_WhenRecipeIsNull()
        {
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.Recipe = null;

            var method = typeof(RecipeEdit).GetMethod("SaveAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            _recipeServiceMock.Verify(s => s.AddAsync(It.IsAny<RecipeEditModel>(), CancellationToken.None), Times.Never);
            _recipeServiceMock.Verify(s => s.UpdateAsync(It.IsAny<RecipeEditModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task SaveAsync_DelegatesToSaveCoreAsync_WhenRecipeIsSet()
        {
            ArrangeLookups();
            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            _recipeServiceMock
                .Setup(s => s.AddAsync(It.IsAny<RecipeEditModel>(), CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent("0");
            cut.Instance.Recipe!.Name = "New Recipe";

            var method = typeof(RecipeEdit).GetMethod("SaveAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            _recipeServiceMock.Verify(
                s => s.AddAsync(It.Is<RecipeEditModel>(r => r.Name == "New Recipe"), CancellationToken.None),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/recipesoverview"));
        }

        // ---------- DeleteAsync (dialog confirmation) ----------
        [Test]
        public async Task DeleteAsync_DoesNotDelete_WhenConfirmationDeclined()
        {
            ArrangeLookups();
            var recipeId = Guid.NewGuid();
            var existing = new RecipeEditModel { Id = recipeId, Name = "Loaded" };

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(recipeId, CancellationToken.None))
                .ReturnsAsync(existing);

            var cut = RenderComponent(recipeId.ToString());

            var method = typeof(RecipeEdit).GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            Task workTask = null!;
            await cut.InvokeAsync(() => { workTask = (Task)method!.Invoke(cut.Instance, [])!; });

            ClickConfirmDialogButton(cut, confirm: false);
            await workTask;

            _recipeServiceMock.Verify(s => s.DeleteAsync(It.IsAny<Guid>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task DeleteAsync_Deletes_WhenConfirmationAccepted()
        {
            ArrangeLookups();
            var recipeId = Guid.NewGuid();
            var existing = new RecipeEditModel { Id = recipeId, Name = "Loaded" };
            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(recipeId, CancellationToken.None))
                .ReturnsAsync(existing);

            _recipeServiceMock
                .Setup(s => s.DeleteAsync(recipeId, CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent(recipeId.ToString());

            var method = typeof(RecipeEdit).GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            Task workTask = null!;
            await cut.InvokeAsync(() => { workTask = (Task)method!.Invoke(cut.Instance, [])!; });

            ClickConfirmDialogButton(cut, confirm: true);
            await workTask;

            _recipeServiceMock.Verify(s => s.DeleteAsync(recipeId, CancellationToken.None), Times.Once);
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/recipesoverview"));
        }
    }
}