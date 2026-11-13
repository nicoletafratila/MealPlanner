using System.Reflection;
using Blazored.Modal;
using Blazored.Modal.Services;
using Bunit;
using Common.Models;
using Common.Pagination;
using Common.Services.Pdf;
using Common.UI;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Pages.MealPlans;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RecipeBook.Services.Http;
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
        private Mock<IPdfBuilder<MealPlanEditModel>> _mealPlanPdfBuilderMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _mealPlanServiceMock = new Mock<IMealPlanService>(MockBehavior.Strict);
            _recipeCategoryServiceMock = new Mock<IRecipeCategoryService>(MockBehavior.Strict);
            _recipeServiceMock = new Mock<IRecipeService>(MockBehavior.Strict);
            _shoppingListServiceMock = new Mock<IShoppingListService>(MockBehavior.Strict);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);
            _mealPlanPdfBuilderMock = new Mock<IPdfBuilder<MealPlanEditModel>>(MockBehavior.Strict);

            _ctx.Services.AddSingleton(_mealPlanServiceMock.Object);
            _ctx.Services.AddSingleton(_recipeCategoryServiceMock.Object);
            _ctx.Services.AddSingleton(_recipeServiceMock.Object);
            _ctx.Services.AddSingleton(_shoppingListServiceMock.Object);
            _ctx.Services.AddSingleton(_messageComponentMock.Object);
            _ctx.Services.AddSingleton(_mealPlanPdfBuilderMock.Object);

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
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([], new Metadata()));

            _mealPlanServiceMock
                .Setup(s => s.GetMenuName(It.IsAny<string>()))
                .Returns("Meniu 2025/23");

            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeModel>([], new Metadata()));
        }

        private IRenderedComponent<MealPlanEdit> RenderComponent(string? id = null, IModalService? modalService = null)
        {
            return _ctx.Render<MealPlanEdit>(ps =>
            {
                if (id is not null)
                    ps.Add(p => p.Id, id);

                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object);

                if (modalService is not null)
                    ps.AddCascadingValue(modalService);
            });
        }

        private static void ClickConfirmDialogButton(IRenderedComponent<MealPlanEdit> cut, bool confirm)
        {
            // MealPlanEdit sets YesButtonColor = Success and NoButtonColor = Danger on its ConfirmDialogOptions.
            var selector = confirm ? "button.btn-success.px-4" : "button.btn-danger.me-md-2.px-4";
            cut.WaitForElement(selector).Click();
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
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cut.Instance.MealPlan!.Id, Is.EqualTo(Guid.Empty));
                Assert.That(cut.Instance.MealPlan!.Recipes, Is.Not.Null);
            }

            _mealPlanServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public void OnInitializedAsync_WithNonZeroId_LoadsMealPlan()
        {
            // Arrange
            ArrangeCategories();

            var id = Guid.NewGuid();
            var existing = new MealPlanEditModel
            {
                Id = id,
                Name = "Loaded Meal Plan",
                Recipes = []
            };

            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);

            // Act
            var cut = RenderComponent(id.ToString());

            // Assert
            Assert.That(cut.Instance.MealPlan, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cut.Instance.MealPlan!.Id, Is.EqualTo(id));
                Assert.That(cut.Instance.MealPlan!.Name, Is.EqualTo("Loaded Meal Plan"));
            }

            _mealPlanServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
        }

        [Test]
        public void OnInitializedAsync_LoadsRecipesWithoutCategoryFilter()
        {
            // Arrange
            ArrangeCategories();

            var recipes = new PagedList<RecipeModel>(
                [new() { Id = Guid.NewGuid(), Name = "Pasta" }],
                new Metadata());

            _recipeServiceMock
                .Setup(s => s.SearchAsync(
                    It.Is<QueryParameters<RecipeModel>>(q =>
                        q.Filters != null &&
                        q.Filters.Count() == 1 &&
                        q.Filters.Any(f => f.PropertyName == "ThumbnailOnly" && Equals(f.Value, true))),
                    CancellationToken.None))
                .ReturnsAsync(recipes);

            // Act
            var cut = RenderComponent("0");

            // Assert: the recipe picker is populated on first load, without needing a category selection
            Assert.That(cut.Instance.Recipes, Is.SameAs(recipes));
        }

        // ---------- SaveCoreAsync ----------
        [Test]
        public async Task SaveCoreAsync_AddsMealPlan_WhenIdIsZero()
        {
            // Arrange
            ArrangeCategories();

            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            _mealPlanServiceMock
                .Setup(s => s.AddAsync(It.IsAny<MealPlanEditModel>(), CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent("0");

            var model = new MealPlanEditModel { Id = Guid.Empty, Name = "New Plan" };

            var method = typeof(MealPlanEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [model])!;
                await task;
            });

            // Assert
            _mealPlanServiceMock.Verify(
                s => s.AddAsync(It.Is<MealPlanEditModel>(m => m.Name == "New Plan"), CancellationToken.None),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Data has been saved successfully", It.IsAny<string>(), CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_UpdatesMealPlan_WhenIdIsNonZero()
        {
            // Arrange
            ArrangeCategories();

            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            var id = Guid.NewGuid();
            var existing = new MealPlanEditModel
            {
                Id = id,
            };

            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);

            _mealPlanServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<MealPlanEditModel>(), CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent(id.ToString());

            var model = new MealPlanEditModel { Id = id, Name = "Updated Plan" };

            var method = typeof(MealPlanEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [model])!;
                await task;
            });

            // Assert
            _mealPlanServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
            _mealPlanServiceMock.Verify(
                s => s.UpdateAsync(It.Is<MealPlanEditModel>(m => m.Id == id), CancellationToken.None),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Data has been saved successfully", It.IsAny<string>(), CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            ArrangeCategories();

            _mealPlanServiceMock
                .Setup(s => s.AddAsync(It.IsAny<MealPlanEditModel>(), CancellationToken.None))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent("0");

            var model = new MealPlanEditModel { Id = Guid.Empty, Name = "New Plan" };

            var method = typeof(MealPlanEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
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
            ArrangeCategories();

            var response = new CommandResponse
            {
                Succeeded = false,
                Message = "Validation error"
            };

            _mealPlanServiceMock
                .Setup(s => s.AddAsync(It.IsAny<MealPlanEditModel>(), CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent("0");

            var model = new MealPlanEditModel { Id = Guid.Empty, Name = "New Plan" };

            var method = typeof(MealPlanEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
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
            ArrangeCategories();
            var cut = RenderComponent("0");
            cut.Instance.MealPlan = new MealPlanEditModel { Id = Guid.Empty };

            var method = typeof(MealPlanEdit).GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _mealPlanServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<Guid>(), CancellationToken.None),
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

            var id = Guid.NewGuid();
            var existing = new MealPlanEditModel
            {
                Id = id,
            };

            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);

            _mealPlanServiceMock
                .Setup(s => s.DeleteAsync(id, CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent(id.ToString());

            var model = new MealPlanEditModel { Id = id };

            var method = typeof(MealPlanEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [model])!;
                await task;
            });

            // Assert
            _mealPlanServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
            _mealPlanServiceMock.Verify(s => s.DeleteAsync(id, CancellationToken.None), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Data has been deleted successfully", It.IsAny<string>(), CancellationToken.None),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("mealplans/mealplansoverview"));
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            ArrangeCategories();

            var id = Guid.NewGuid();
            var existing = new MealPlanEditModel
            {
                Id = id,
            };

            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);

            _mealPlanServiceMock
                .Setup(s => s.DeleteAsync(id, CancellationToken.None))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent(id.ToString());

            var model = new MealPlanEditModel { Id = id };

            var method = typeof(MealPlanEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [model])!;
                await task;
            });

            // Assert
            _mealPlanServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Delete failed. Please try again.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
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

            var id = Guid.NewGuid();
            var existing = new MealPlanEditModel
            {
                Id = id,
            };

            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);

            _mealPlanServiceMock
                .Setup(s => s.DeleteAsync(id, CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent(id.ToString());

            var model = new MealPlanEditModel { Id = id };

            var method = typeof(MealPlanEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [model])!;
                await task;
            });

            // Assert
            _mealPlanServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Delete failed because of dependency", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
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

            var recipeId = Guid.NewGuid();
            var recipe = new RecipeModel { Id = recipeId, Name = "R1" };

            _recipeServiceMock
                .Setup(s => s.GetByIdAsync(recipeId, CancellationToken.None))
                .ReturnsAsync(recipe);

            var cut = RenderComponent("0");

            cut.Instance.MealPlan.Recipes = [];
            cut.Instance.RecipeId = recipeId.ToString();

            var method = typeof(MealPlanEdit).GetMethod("AddRecipeAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.MealPlan.Recipes!, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cut.Instance.MealPlan.Recipes![0].Id, Is.EqualTo(recipeId));
                Assert.That(cut.Instance.MealPlan.Recipes![0].Index, Is.EqualTo(1));
            }
        }

        // ---------- SaveAsync ----------
        [Test]
        public async Task SaveAsync_AddsMealPlan_UsingCurrentMealPlan()
        {
            // Arrange
            ArrangeCategories();

            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            _mealPlanServiceMock
                .Setup(s => s.AddAsync(It.IsAny<MealPlanEditModel>(), CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent("0");
            cut.Instance.MealPlan.Name = "Button Save";

            var method = typeof(MealPlanEdit).GetMethod("SaveAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _mealPlanServiceMock.Verify(
                s => s.AddAsync(It.Is<MealPlanEditModel>(m => m.Name == "Button Save"), CancellationToken.None),
                Times.Once);
        }

        // ---------- DeleteAsync (full dialog flow) ----------
        [Test]
        public async Task DeleteAsync_DeletesMealPlan_WhenConfirmationAccepted()
        {
            // Arrange
            ArrangeCategories();

            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            var id = Guid.NewGuid();
            var existing = new MealPlanEditModel { Id = id };

            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);

            _mealPlanServiceMock
                .Setup(s => s.DeleteAsync(id, CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderComponent(id.ToString());

            var method = typeof(MealPlanEdit).GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            Task workTask = null!;
            await cut.InvokeAsync(() => { workTask = (Task)method!.Invoke(cut.Instance, [])!; });

            ClickConfirmDialogButton(cut, confirm: true);
            await workTask;

            // Assert
            _mealPlanServiceMock.Verify(s => s.DeleteAsync(id, CancellationToken.None), Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("mealplans/mealplansoverview"));
        }

        [Test]
        public async Task DeleteAsync_KeepsMealPlan_WhenConfirmationDeclined()
        {
            // Arrange
            ArrangeCategories();

            var id = Guid.NewGuid();
            var existing = new MealPlanEditModel { Id = id };

            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);

            var cut = RenderComponent(id.ToString());

            var method = typeof(MealPlanEdit).GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            Task workTask = null!;
            await cut.InvokeAsync(() => { workTask = (Task)method!.Invoke(cut.Instance, [])!; });

            ClickConfirmDialogButton(cut, confirm: false);
            await workTask;

            // Assert
            _mealPlanServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
        }

        // ---------- PrintAsync ----------
        [Test]
        public async Task PrintAsync_BuildsPdf_AndInvokesDownload_WithSanitizedFileName()
        {
            // Arrange
            ArrangeCategories();

            var cut = RenderComponent("0");
            cut.Instance.MealPlan = new MealPlanEditModel { Id = Guid.NewGuid(), Name = "Weekly:Menu", Recipes = [] };

            var pdfBytes = new byte[] { 1, 2, 3, 4 };
            _mealPlanPdfBuilderMock
                .Setup(b => b.Build(cut.Instance.MealPlan))
                .Returns(pdfBytes);

            _ctx.JSInterop.SetupVoid("downloadFileFromBase64", _ => true).SetVoidResult();

            var method = typeof(MealPlanEdit).GetMethod("PrintAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _mealPlanPdfBuilderMock.Verify(b => b.Build(cut.Instance.MealPlan), Times.Once);

            var invocation = _ctx.JSInterop.Invocations["downloadFileFromBase64"].Single();
            Assert.That(invocation.Arguments[0], Is.EqualTo("Weekly_Menu.pdf"));
            Assert.That(invocation.Arguments[1], Is.EqualTo(Convert.ToBase64String(pdfBytes)));
        }

        [Test]
        public async Task PrintAsync_DoesNothing_WhenMealPlanIsNull()
        {
            // Arrange
            ArrangeCategories();

            var cut = RenderComponent("0");
            cut.Instance.MealPlan = null!;

            var method = typeof(MealPlanEdit).GetMethod("PrintAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _mealPlanPdfBuilderMock.Verify(b => b.Build(It.IsAny<MealPlanEditModel>()), Times.Never);
            Assert.That(_ctx.JSInterop.Invocations.Any(i => i.Identifier == "downloadFileFromBase64"), Is.False);
        }

        // ---------- SaveShoppingListAsync ----------
        [Test]
        public async Task SaveShoppingListAsync_ReturnsEarly_WhenNoRecipes()
        {
            // Arrange
            ArrangeCategories();

            var modalServiceMock = new Mock<IModalService>(MockBehavior.Strict);
            var cut = RenderComponent("0", modalServiceMock.Object);
            cut.Instance.MealPlan.Recipes = [];

            var method = typeof(MealPlanEdit).GetMethod("SaveShoppingListAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _shoppingListServiceMock.Verify(
                s => s.MakeShoppingListAsync(It.IsAny<ShoppingListCreateModel>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task SaveShoppingListAsync_ReturnsEarly_WhenModalServiceNotAvailable()
        {
            // Arrange
            ArrangeCategories();

            var cut = RenderComponent("0");
            cut.Instance.MealPlan.Recipes = [new RecipeModel { Id = Guid.NewGuid(), Name = "R1" }];

            var method = typeof(MealPlanEdit).GetMethod("SaveShoppingListAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _shoppingListServiceMock.Verify(
                s => s.MakeShoppingListAsync(It.IsAny<ShoppingListCreateModel>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task SaveShoppingListAsync_ReturnsEarly_WhenModalCancelled()
        {
            // Arrange
            ArrangeCategories();

            var modalReferenceMock = new Mock<IModalReference>(MockBehavior.Strict);
            modalReferenceMock.Setup(m => m.Result).Returns(Task.FromResult(ModalResult.Cancel()));

            var modalServiceMock = new Mock<IModalService>(MockBehavior.Strict);
            modalServiceMock.Setup(m => m.Show<ShopSelection>(It.IsAny<string>())).Returns(modalReferenceMock.Object);

            var cut = RenderComponent("0", modalServiceMock.Object);
            cut.Instance.MealPlan.Recipes = [new RecipeModel { Id = Guid.NewGuid(), Name = "R1" }];

            var method = typeof(MealPlanEdit).GetMethod("SaveShoppingListAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _shoppingListServiceMock.Verify(
                s => s.MakeShoppingListAsync(It.IsAny<ShoppingListCreateModel>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task SaveShoppingListAsync_ReturnsEarly_WhenSelectedShopIsNotAGuid()
        {
            // Arrange
            ArrangeCategories();

            var modalReferenceMock = new Mock<IModalReference>(MockBehavior.Strict);
            modalReferenceMock.Setup(m => m.Result).Returns(Task.FromResult(ModalResult.Ok("not-a-guid")));

            var modalServiceMock = new Mock<IModalService>(MockBehavior.Strict);
            modalServiceMock.Setup(m => m.Show<ShopSelection>(It.IsAny<string>())).Returns(modalReferenceMock.Object);

            var cut = RenderComponent("0", modalServiceMock.Object);
            cut.Instance.MealPlan.Recipes = [new RecipeModel { Id = Guid.NewGuid(), Name = "R1" }];

            var method = typeof(MealPlanEdit).GetMethod("SaveShoppingListAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _shoppingListServiceMock.Verify(
                s => s.MakeShoppingListAsync(It.IsAny<ShoppingListCreateModel>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task SaveShoppingListAsync_NavigatesToNewShoppingList_WhenConfirmed()
        {
            // Arrange
            ArrangeCategories();

            var mealPlanId = Guid.NewGuid();
            var shopId = Guid.NewGuid();
            var newShoppingListId = Guid.NewGuid();

            var modalReferenceMock = new Mock<IModalReference>(MockBehavior.Strict);
            modalReferenceMock.Setup(m => m.Result).Returns(Task.FromResult(ModalResult.Ok(shopId.ToString())));

            var modalServiceMock = new Mock<IModalService>(MockBehavior.Strict);
            modalServiceMock.Setup(m => m.Show<ShopSelection>(It.IsAny<string>())).Returns(modalReferenceMock.Object);

            var existing = new MealPlanEditModel { Id = mealPlanId, Recipes = [new RecipeModel { Id = Guid.NewGuid(), Name = "R1" }] };

            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(mealPlanId, CancellationToken.None))
                .ReturnsAsync(existing);

            _shoppingListServiceMock
                .Setup(s => s.MakeShoppingListAsync(
                    It.Is<ShoppingListCreateModel>(m => m.MealPlanId == mealPlanId && m.ShopId == shopId),
                    CancellationToken.None))
                .ReturnsAsync(new ShoppingListEditModel { Id = newShoppingListId });

            var cut = RenderComponent(mealPlanId.ToString(), modalServiceMock.Object);

            var method = typeof(MealPlanEdit).GetMethod("SaveShoppingListAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith($"mealplans/shoppinglistedit/{newShoppingListId}"));
        }

        [Test]
        public async Task SaveShoppingListAsync_ShowsError_WhenMakeShoppingListFails()
        {
            // Arrange
            ArrangeCategories();

            var mealPlanId = Guid.NewGuid();
            var shopId = Guid.NewGuid();

            var modalReferenceMock = new Mock<IModalReference>(MockBehavior.Strict);
            modalReferenceMock.Setup(m => m.Result).Returns(Task.FromResult(ModalResult.Ok(shopId.ToString())));

            var modalServiceMock = new Mock<IModalService>(MockBehavior.Strict);
            modalServiceMock.Setup(m => m.Show<ShopSelection>(It.IsAny<string>())).Returns(modalReferenceMock.Object);

            var existing = new MealPlanEditModel { Id = mealPlanId, Recipes = [new RecipeModel { Id = Guid.NewGuid(), Name = "R1" }] };

            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(mealPlanId, CancellationToken.None))
                .ReturnsAsync(existing);

            _shoppingListServiceMock
                .Setup(s => s.MakeShoppingListAsync(It.IsAny<ShoppingListCreateModel>(), CancellationToken.None))
                .ReturnsAsync((ShoppingListEditModel?)null);

            var cut = RenderComponent(mealPlanId.ToString(), modalServiceMock.Object);

            var method = typeof(MealPlanEdit).GetMethod("SaveShoppingListAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("There has been an error when saving the shopping list", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }

        // ---------- DeleteRecipeAsync ----------
        [Test]
        public async Task DeleteRecipeAsync_DoesNothing_WhenRecipeNotInList()
        {
            // Arrange
            ArrangeCategories();
            var cut = RenderComponent("0");
            cut.Instance.MealPlan.Recipes = [];

            var otherRecipe = new RecipeModel { Id = Guid.NewGuid(), Name = "Missing" };

            var method = typeof(MealPlanEdit).GetMethod("DeleteRecipeAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [otherRecipe])!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.MealPlan.Recipes, Is.Empty);
        }

        [Test]
        public async Task DeleteRecipeAsync_KeepsRecipe_WhenConfirmationDeclined()
        {
            // Arrange
            ArrangeCategories();
            var cut = RenderComponent("0");
            var recipe = new RecipeModel { Id = Guid.NewGuid(), Name = "Pasta" };
            cut.Instance.MealPlan.Recipes = [recipe];

            var method = typeof(MealPlanEdit).GetMethod("DeleteRecipeAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            Task workTask = null!;
            await cut.InvokeAsync(() => { workTask = (Task)method!.Invoke(cut.Instance, [recipe])!; });

            ClickConfirmDialogButton(cut, confirm: false);
            await workTask;

            // Assert
            Assert.That(cut.Instance.MealPlan.Recipes, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task DeleteRecipeAsync_RemovesRecipe_WhenConfirmationAccepted()
        {
            // Arrange
            ArrangeCategories();
            var cut = RenderComponent("0");
            var recipe = new RecipeModel { Id = Guid.NewGuid(), Name = "Pasta" };
            cut.Instance.MealPlan.Recipes = [recipe];

            var method = typeof(MealPlanEdit).GetMethod("DeleteRecipeAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            Task workTask = null!;
            await cut.InvokeAsync(() => { workTask = (Task)method!.Invoke(cut.Instance, [recipe])!; });

            ClickConfirmDialogButton(cut, confirm: true);
            await workTask;

            // Assert
            Assert.That(cut.Instance.MealPlan.Recipes, Is.Empty);
        }

        // ---------- NavigateToOverview ----------
        [Test]
        public void NavigateToOverview_NavigatesToMealPlansOverview()
        {
            // Arrange
            ArrangeCategories();
            var cut = RenderComponent("0");

            var method = typeof(MealPlanEdit).GetMethod("NavigateToOverview", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            cut.InvokeAsync(() => method!.Invoke(cut.Instance, []));

            // Assert
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("mealplans/mealplansoverview"));
        }

        // ---------- OnRecipeCategoryChangedAsync ----------
        [Test]
        public async Task OnRecipeCategoryChangedAsync_BuildsFilters_AndResetsRecipeId()
        {
            // Arrange
            ArrangeCategories();

            var recipes = new PagedList<RecipeModel>([new() { Id = Guid.NewGuid(), Name = "A" }], new Metadata());

            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), CancellationToken.None))
                .ReturnsAsync(recipes);

            var cut = RenderComponent("0");

            var method = typeof(MealPlanEdit).GetMethod("OnRecipeCategoryChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var args = new ChangeEventArgs { Value = "3" };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [args])!;
                await task;
            });

            // Assert
            _recipeServiceMock.Verify(
                s => s.SearchAsync(It.Is<QueryParameters<RecipeModel>>(qp =>
                    qp.Filters != null &&
                    qp.Filters.Count() == 2 &&
                    qp.Filters.Any(f => f.PropertyName == "ThumbnailOnly" && Equals(f.Value, true)) &&
                    qp.Filters.Any(f => f.PropertyName == "RecipeCategoryId" && f.Value as string == "3")), CancellationToken.None),
                Times.Once);

            Assert.That(cut.Instance.RecipeId, Is.EqualTo(string.Empty));
        }
    }
}