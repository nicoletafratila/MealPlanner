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
    public class MealPlanEditTests
    {
        private BunitContext _ctx = null!;
        private Mock<IMealPlanService> _mealPlanServiceMock = null!;
        private Mock<IRecipeCategoryService> _recipeCategoryServiceMock = null!;
        private Mock<IRecipeService> _recipeServiceMock = null!;
        private Mock<IShoppingListService> _shoppingListServiceMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _mealPlanServiceMock = new Mock<IMealPlanService>(MockBehavior.Strict);
            _recipeCategoryServiceMock = new Mock<IRecipeCategoryService>(MockBehavior.Strict);
            _recipeServiceMock = new Mock<IRecipeService>(MockBehavior.Strict);
            _shoppingListServiceMock = new Mock<IShoppingListService>(MockBehavior.Strict);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_mealPlanServiceMock.Object);
            _ctx.Services.AddSingleton(_recipeCategoryServiceMock.Object);
            _ctx.Services.AddSingleton(_recipeServiceMock.Object);
            _ctx.Services.AddSingleton(_shoppingListServiceMock.Object);
            _ctx.Services.AddSingleton(_messageComponentMock.Object);

            _ctx.Services.AddBlazoredModal();
            _ctx.Services.AddBlazorBootstrap();
            _ctx.Services.AddLogging();

            _ctx.JSInterop.SetupVoid("checkQuantity", _ => true).SetVoidResult();
            _ctx.JSInterop
                .SetupVoid("window.blazorBootstrap.offcanvas.initialize", _ => true)
                .SetVoidResult();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
        }

        private void ArrangeCategories()
        {
            _recipeCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([], new Metadata()));
        }

        private IRenderedComponent<MealPlanEdit> RenderComponent(string? id = null)
        {
            return _ctx.Render<MealPlanEdit>(ps =>
            {
                if (id is not null)
                    ps.Add(p => p.Id, id);

                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
            });
        }

        // ---------- OnInitializedAsync ----------
        [Test]
        public void OnInitializedAsync_WithIdZero_CreatesNewMealPlan()
        {
            // Arrange
            ArrangeCategories();

            // Act
            var cut = RenderComponent("0");

            // Assert
            Assert.That(cut.Instance.MealPlan, Is.Not.Null);
            Assert.That(cut.Instance.MealPlan!.Id, Is.EqualTo(0));
            Assert.That(cut.Instance.MealPlan!.Recipes, Is.Not.Null);

            _mealPlanServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public void OnInitializedAsync_WithNonZeroId_LoadsMealPlan()
        {
            // Arrange
            ArrangeCategories();

            var existing = new MealPlanEditModel
            {
                Id = 5,
                Name = "Loaded Meal Plan",
                Recipes = new List<RecipeModel>()
            };

            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            // Act
            var cut = RenderComponent("5");

            // Assert
            Assert.That(cut.Instance.MealPlan, Is.Not.Null);
            Assert.That(cut.Instance.MealPlan!.Id, Is.EqualTo(5));
            Assert.That(cut.Instance.MealPlan!.Name, Is.EqualTo("Loaded Meal Plan"));

            _mealPlanServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
        }

        // ---------- SaveCoreAsync ----------
        [Test]
        public async Task SaveCoreAsync_AddsMealPlan_WhenIdIsZero()
        {
            // Arrange
            ArrangeCategories();

            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            _mealPlanServiceMock
                .Setup(s => s.AddAsync(It.IsAny<MealPlanEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent("0");

            var model = new MealPlanEditModel { Id = 0, Name = "New Plan" };

            var method = typeof(MealPlanEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { model })!;
                await task;
            });

            // Assert
            _mealPlanServiceMock.Verify(
                s => s.AddAsync(It.Is<MealPlanEditModel>(m => m.Name == "New Plan")),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been saved successfully"),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri.EndsWith("mealplans/mealplansoverview"), Is.True);
        }

        [Test]
        public async Task SaveCoreAsync_UpdatesMealPlan_WhenIdIsNonZero()
        {
            // Arrange
            ArrangeCategories();

            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            var existing = new MealPlanEditModel
            {
                Id = 5,
            };

            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _mealPlanServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<MealPlanEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent("5");

            var model = new MealPlanEditModel { Id = 5, Name = "Updated Plan" };

            var method = typeof(MealPlanEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { model })!;
                await task;
            });

            // Assert
            _mealPlanServiceMock.Verify(s => s.GetEditAsync(5), Times.Once); 
            _mealPlanServiceMock.Verify(
                s => s.UpdateAsync(It.Is<MealPlanEditModel>(m => m.Id == 5)),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been saved successfully"),
                Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            ArrangeCategories();

            _mealPlanServiceMock
                .Setup(s => s.AddAsync(It.IsAny<MealPlanEditModel>()))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent("0");

            var model = new MealPlanEditModel { Id = 0, Name = "New Plan" };

            var method = typeof(MealPlanEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { model })!;
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
            ArrangeCategories();

            var response = new CommandResponse
            {
                Succeeded = false,
                Message = "Validation error"
            };

            _mealPlanServiceMock
                .Setup(s => s.AddAsync(It.IsAny<MealPlanEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent("0");

            var model = new MealPlanEditModel { Id = 0, Name = "New Plan" };

            var method = typeof(MealPlanEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { model })!;
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
            ArrangeCategories();
            var cut = RenderComponent("0");
            cut.Instance.MealPlan = new MealPlanEditModel { Id = 0 };

            var method = typeof(MealPlanEdit).GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, Array.Empty<object>())!;
                await task;
            });

            // Assert
            _mealPlanServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public async Task DeleteCoreAsync_Deletes_WhenResponseSucceeded()
        {
            // Arrange
            ArrangeCategories();

            var response = new CommandResponse
            {
                Succeeded = true,
                Message = "ok"
            };

            var existing = new MealPlanEditModel
            {
                Id = 5,
            };

            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _mealPlanServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync(response);

            var cut = RenderComponent("5");

            var model = new MealPlanEditModel { Id = 5 };

            var method = typeof(MealPlanEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { model })!;
                await task;
            });

            // Assert
            _mealPlanServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _mealPlanServiceMock.Verify(s => s.DeleteAsync(5), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been deleted successfully"),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri.EndsWith("mealplans/mealplansoverview"), Is.True);
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            ArrangeCategories();

            var existing = new MealPlanEditModel
            {
                Id = 5,
            };

            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _mealPlanServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent("5");

            var model = new MealPlanEditModel { Id = 5 };

            var method = typeof(MealPlanEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { model })!;
                await task;
            });

            // Assert
            _mealPlanServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowError("Delete failed. Please try again."),
                Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsResponseMessage_WhenFailed()
        {
            // Arrange
            ArrangeCategories();

            var response = new CommandResponse
            {
                Succeeded = false,
                Message = "Delete failed because of dependency"
            };

            var existing = new MealPlanEditModel
            {
                Id = 5,
            };

            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _mealPlanServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync(response);

            var cut = RenderComponent("5");

            var model = new MealPlanEditModel { Id = 5 };

            var method = typeof(MealPlanEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { model })!;
                await task;
            });

            // Assert
            _mealPlanServiceMock.Verify(s => s.GetEditAsync(5), Times.Once); 
            _messageComponentMock.Verify(
                m => m.ShowError("Delete failed because of dependency"),
                Times.Once);
        }

        // ---------- CanAddRecipe ----------
        [Test]
        public void CanAddRecipe_False_WhenRequiredFieldsMissing()
        {
            // Arrange
            ArrangeCategories();
            var cut = RenderComponent("0");

            cut.Instance.RecipeId = "0";

            var prop = typeof(MealPlanEdit).GetProperty("CanAddRecipe", BindingFlags.Instance | BindingFlags.NonPublic);
            var result = (bool)prop!.GetValue(cut.Instance)!;

            Assert.That(result, Is.False);
        }

        [Test]
        public void CanAddRecipe_True_WhenRecipeIdValid()
        {
            // Arrange
            ArrangeCategories();
            var cut = RenderComponent("0");

            cut.Instance.RecipeId = "10";

            var prop = typeof(MealPlanEdit).GetProperty("CanAddRecipe", BindingFlags.Instance | BindingFlags.NonPublic);
            var result = (bool)prop!.GetValue(cut.Instance)!;

            Assert.That(result, Is.True);
        }

        // ---------- AddRecipeAsync ----------
        [Test]
        public async Task AddRecipeAsync_AddsNewRecipe_AndSetsIndexes()
        {
            // Arrange
            ArrangeCategories();

            var recipe = new RecipeModel { Id = 5, Name = "R1" };

            _recipeServiceMock
                .Setup(s => s.GetByIdAsync(5))
                .ReturnsAsync(recipe);

            var cut = RenderComponent("0");

            cut.Instance.MealPlan.Recipes = new List<RecipeModel>();
            cut.Instance.RecipeId = "5";

            var method = typeof(MealPlanEdit).GetMethod("AddRecipeAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, Array.Empty<object>())!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.MealPlan.Recipes!.Count, Is.EqualTo(1));
            Assert.That(cut.Instance.MealPlan.Recipes![0].Id, Is.EqualTo(5));
            Assert.That(cut.Instance.MealPlan.Recipes![0].Index, Is.EqualTo(1));
        }

        // ---------- OnRecipeCategoryChangedAsync ----------
        [Test]
        public async Task OnRecipeCategoryChangedAsync_BuildsFilters_AndResetsRecipeId()
        {
            // Arrange
            ArrangeCategories();

            var recipes = new PagedList<RecipeModel>(
                new List<RecipeModel> { new() { Id = 1, Name = "A" } },
                new Metadata());

            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>()))
                .ReturnsAsync(recipes);

            var cut = RenderComponent("0");

            var method = typeof(MealPlanEdit).GetMethod("OnRecipeCategoryChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var args = new ChangeEventArgs { Value = "3" };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { args })!;
                await task;
            });

            // Assert
            _recipeServiceMock.Verify(
                s => s.SearchAsync(It.Is<QueryParameters<RecipeModel>>(qp =>
                    qp.Filters != null &&
                    qp.Filters.Count() == 1 &&
                    qp.Filters.First().PropertyName == "RecipeCategoryId" &&
                    (string)qp.Filters.First().Value == "3")),
                Times.Once);

            Assert.That(cut.Instance.RecipeId, Is.EqualTo(string.Empty));
        }
    }
}