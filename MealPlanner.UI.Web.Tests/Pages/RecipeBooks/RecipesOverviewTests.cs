using System.Reflection;
using BlazorBootstrap;
using Blazored.SessionStorage;
using Bunit;
using Common.Models;
using Common.Pagination;
using Common.UI;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Pages.RecipeBooks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Tests.Pages.RecipeBooks
{
    [TestFixture]
    public class RecipesOverviewTests
    {
        private const string EditBaseUrl = "recipebooks/recipeedit/";

        private BunitContext _ctx = null!;
        private Mock<IRecipeService> _recipeServiceMock = null!;
        private Mock<IMealPlanService> _mealPlanServiceMock = null!;
        private Mock<ISessionStorageService> _sessionStorageMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _recipeServiceMock = new Mock<IRecipeService>(MockBehavior.Strict);
            _mealPlanServiceMock = new Mock<IMealPlanService>(MockBehavior.Loose);
            _sessionStorageMock = new Mock<ISessionStorageService>(MockBehavior.Strict);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_recipeServiceMock.Object);
            _ctx.Services.AddSingleton(_mealPlanServiceMock.Object);
            _ctx.Services.AddSingleton(_sessionStorageMock.Object);

            _ctx.Services.AddScoped<BreadcrumbService>();
            _ctx.Services.AddScoped<ModalService>();
            _ctx.Services.AddScoped<PreloadService>();

            _ctx.JSInterop.SetupVoid("window.blazorBootstrap.dropdown.initialize", _ => true);

            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeModel>([], new Metadata()));

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
            _recipeServiceMock.Reset();
            _mealPlanServiceMock.Reset();
            _sessionStorageMock.Reset();
            _messageComponentMock.Reset();
        }

        private IRenderedComponent<RecipesOverview> RenderWithMessageComponent()
        {
            return _ctx.Render<RecipesOverview>(parameters =>
            {
                parameters.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
            });
        }

        private IRenderedComponent<RecipesOverview> RenderWithRefreshCallback(Action onRefresh)
        {
            Func<Task> callback = () => { onRefresh(); return Task.CompletedTask; };
            return _ctx.Render<RecipesOverview>(parameters =>
            {
                parameters.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
                parameters.AddCascadingValue("RefreshCurrentMealPlan", callback);
            });
        }

        private static async Task InvokeAddToMealPlanAsync(IRenderedComponent<RecipesOverview> cut, RecipeModel recipe)
        {
            var method = typeof(RecipesOverview).GetMethod("AddToMealPlanAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "AddToMealPlanAsync not found via reflection.");
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [recipe])!;
                await task;
            });
        }

        // ---------- Navigation ----------
        [Test]
        public void New_NavigatesToCreatePage()
        {
            // Arrange
            var navManager = _ctx.Services.GetRequiredService<NavigationManager>();
            var cut = RenderWithMessageComponent();

            // Act
            cut.InvokeAsync(() =>
            {
                var m = typeof(RecipesOverview).GetMethod("New", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(m, Is.Not.Null);
                m!.Invoke(cut.Instance, []);
            });

            // Assert
            Assert.That(navManager.Uri, Does.EndWith(EditBaseUrl));
        }

        [Test]
        public void Update_NavigatesToEditPage_ForGivenItem()
        {
            // Arrange
            var navManager = _ctx.Services.GetRequiredService<NavigationManager>();
            var cut = RenderWithMessageComponent();

            var method = typeof(RecipesOverview).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var model = new RecipeModel { Id = 42 };

            // Act
            cut.InvokeAsync(() => method!.Invoke(cut.Instance, [model]));

            // Assert
            Assert.That(navManager.Uri, Does.EndWith($"{EditBaseUrl}42"));
        }

        // ---------- DeleteCoreAsync (core delete logic) ----------
        [Test]
        public async Task DeleteCoreAsync_WhenDeleteSucceeds_ShowsInfo_AndRefreshesGrid()
        {
            // Arrange
            var recipe = new RecipeModel { Id = 5 };

            _recipeServiceMock
                .Setup(s => s.DeleteAsync(recipe.Id, CancellationToken.None))
                .ReturnsAsync(new CommandResponse { Succeeded = true, Message = "ok" });

            var cut = RenderWithMessageComponent();

            var method = typeof(RecipesOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "DeleteCoreAsync method not found via reflection.");

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [recipe])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Data has been deleted successfully", It.IsAny<string>(), CancellationToken.None),
                Times.Once);

            _recipeServiceMock.Verify(s => s.DeleteAsync(recipe.Id, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_WhenDeleteFails_ShowsError()
        {
            // Arrange
            var recipe = new RecipeModel { Id = 7 };
            var response = new CommandResponse { Succeeded = false, Message = "delete failed" };

            _recipeServiceMock
                .Setup(s => s.DeleteAsync(recipe.Id, CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderWithMessageComponent();

            var method = typeof(RecipesOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "DeleteCoreAsync method not found via reflection.");

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [recipe])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(m => m.ShowErrorAsync("delete failed", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None), Times.Once);
            _recipeServiceMock.Verify(s => s.DeleteAsync(recipe.Id, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_WhenResponseIsNull_ShowsGenericError()
        {
            // Arrange
            var recipe = new RecipeModel { Id = 9 };

            _recipeServiceMock
                .Setup(s => s.DeleteAsync(recipe.Id, CancellationToken.None))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderWithMessageComponent();

            var method = typeof(RecipesOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [recipe])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(m => m.ShowErrorAsync("Delete failed. Please try again.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None), Times.Once);
        }

        // ---------- DataProviderAsync ----------
        [Test]
        public async Task DataProviderAsync_CallsService_SavesQuery_InSessionStorage_AndReturnsData()
        {
            // Arrange
            var items = new List<RecipeModel>
            {
                new() { Id = 1 },
                new() { Id = 2 },
            };

            var metadata = new Metadata
            {
                PageNumber = 1,
                PageSize = 20,
                TotalCount = 2
            };

            var paged = new PagedList<RecipeModel>(items, metadata);

            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), CancellationToken.None))
                .ReturnsAsync(paged);

            var cut = RenderWithMessageComponent();

            var method = typeof(RecipesOverview).GetMethod("DataProviderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var request = new GridDataProviderRequest<RecipeModel>
            {
                PageNumber = 1,
                PageSize = 20
            };

            // Act
            GridDataProviderResult<RecipeModel> result = await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<RecipeModel>>)method!
                    .Invoke(cut.Instance, [request])!;
                return await task;
            });

            // Assert
            _recipeServiceMock.Verify(
                s => s.SearchAsync(It.Is<QueryParameters<RecipeModel>>(q =>
                    q.PageNumber == 1 && q.PageSize == 20), CancellationToken.None),
                Times.Exactly(2));

            _sessionStorageMock.Verify(
                s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Data!.Count(), Is.EqualTo(2));
                Assert.That(result.TotalCount, Is.EqualTo(2));
            }
        }

        // ---------- AddToMealPlanAsync ----------

        [Test]
        public async Task AddToMealPlanAsync_WhenNoCurrentPlan_CreatesPlanWithRecipeAndShowsCreatedMessage()
        {
            // Arrange
            var recipe = new RecipeModel { Id = 1, Name = "Pasta" };

            _mealPlanServiceMock
                .Setup(s => s.GetCurrentAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((MealPlanModel?)null);
            _mealPlanServiceMock
                .Setup(s => s.GetMenuName(It.IsAny<string>()))
                .Returns("Meniu 2025/23");
            _mealPlanServiceMock
                .Setup(s => s.AddAsync(It.IsAny<MealPlanEditModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CommandResponse { Succeeded = true });

            var cut = RenderWithMessageComponent();

            // Act
            await InvokeAddToMealPlanAsync(cut, recipe);

            // Assert — plan created with the recipe included
            _mealPlanServiceMock.Verify(
                s => s.AddAsync(
                    It.Is<MealPlanEditModel>(m =>
                        m.Name == "Meniu 2025/23" &&
                        m.Recipes != null &&
                        m.Recipes.Count == 1 &&
                        m.Recipes[0].Id == recipe.Id),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync(
                    It.Is<string>(s => s.Contains("created")),
                    It.IsAny<string>(),
                    CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task AddToMealPlanAsync_WhenNoCurrentPlan_InvokesRefreshCallback()
        {
            // Arrange
            var recipe = new RecipeModel { Id = 2 };
            var refreshCalled = false;

            _mealPlanServiceMock
                .Setup(s => s.GetCurrentAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((MealPlanModel?)null);
            _mealPlanServiceMock
                .Setup(s => s.GetMenuName(It.IsAny<string>()))
                .Returns("Meniu 2025/23");
            _mealPlanServiceMock
                .Setup(s => s.AddAsync(It.IsAny<MealPlanEditModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CommandResponse { Succeeded = true });

            var cut = RenderWithRefreshCallback(() => refreshCalled = true);

            // Act
            await InvokeAddToMealPlanAsync(cut, recipe);

            // Assert
            Assert.That(refreshCalled, Is.True);
        }

        [Test]
        public async Task AddToMealPlanAsync_WhenNoCurrentPlan_AndCreateFails_ShowsError()
        {
            // Arrange
            var recipe = new RecipeModel { Id = 3 };

            _mealPlanServiceMock
                .Setup(s => s.GetCurrentAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((MealPlanModel?)null);
            _mealPlanServiceMock
                .Setup(s => s.GetMenuName(It.IsAny<string>()))
                .Returns("Meniu 2025/23");
            _mealPlanServiceMock
                .Setup(s => s.AddAsync(It.IsAny<MealPlanEditModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CommandResponse { Succeeded = false, Message = "save error" });

            var cut = RenderWithMessageComponent();

            // Act
            await InvokeAddToMealPlanAsync(cut, recipe);

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("save error", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
            _mealPlanServiceMock.Verify(
                s => s.UpdateAsync(It.IsAny<MealPlanEditModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task AddToMealPlanAsync_WhenCurrentPlanExists_AddsRecipeAndShowsInfo()
        {
            // Arrange
            var recipe = new RecipeModel { Id = 5, Name = "Salad" };
            var currentPlan = new MealPlanModel { Id = 10, Name = "This week" };
            var editModel = new MealPlanEditModel { Id = 10, Name = "This week", Recipes = [] };

            _mealPlanServiceMock
                .Setup(s => s.GetCurrentAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentPlan);
            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(editModel);
            _mealPlanServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<MealPlanEditModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CommandResponse { Succeeded = true });

            var cut = RenderWithMessageComponent();

            // Act
            await InvokeAddToMealPlanAsync(cut, recipe);

            // Assert
            _mealPlanServiceMock.Verify(
                s => s.UpdateAsync(
                    It.Is<MealPlanEditModel>(m => m.Recipes!.Any(r => r.Id == recipe.Id)),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync(
                    "Recipe has been added successfully",
                    It.IsAny<string>(),
                    CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task AddToMealPlanAsync_WhenCurrentPlanExists_AndRecipeAlreadyAdded_DoesNotUpdate()
        {
            // Arrange
            var recipe = new RecipeModel { Id = 7 };
            var currentPlan = new MealPlanModel { Id = 10 };
            var editModel = new MealPlanEditModel
            {
                Id = 10,
                Recipes = [new RecipeModel { Id = 7 }]
            };

            _mealPlanServiceMock
                .Setup(s => s.GetCurrentAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentPlan);
            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(editModel);

            var cut = RenderWithMessageComponent();

            // Act
            await InvokeAddToMealPlanAsync(cut, recipe);

            // Assert — duplicate silently ignored
            _mealPlanServiceMock.Verify(
                s => s.UpdateAsync(It.IsAny<MealPlanEditModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _messageComponentMock.Verify(
                m => m.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task AddToMealPlanAsync_WhenCurrentPlanExists_AndUpdateFails_ShowsError()
        {
            // Arrange
            var recipe = new RecipeModel { Id = 8 };
            var currentPlan = new MealPlanModel { Id = 10 };
            var editModel = new MealPlanEditModel { Id = 10, Recipes = [] };

            _mealPlanServiceMock
                .Setup(s => s.GetCurrentAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentPlan);
            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(editModel);
            _mealPlanServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<MealPlanEditModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CommandResponse { Succeeded = false, Message = "update failed" });

            var cut = RenderWithMessageComponent();

            // Act
            await InvokeAddToMealPlanAsync(cut, recipe);

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("update failed", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }
    }
}