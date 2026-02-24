using System.Reflection;
using Bunit;
using Common.Models;
using Common.Pagination;
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
            _ctx.Services.AddSingleton<IMessageComponent>(_messageComponentMock.Object);

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
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([], new Metadata()));

            _productCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], new Metadata()));

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>()))
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

        // ---------- OnInitializedAsync ----------
        [Test]
        public void OnInitializedAsync_WithIdZero_CreatesNewRecipe()
        {
            // Arrange
            ArrangeLookups();

            // Act
            var cut = RenderComponent("0");

            // Assert
            Assert.That(cut.Instance.Recipe, Is.Not.Null);
            Assert.That(cut.Instance.Recipe!.Id, Is.EqualTo(0));

            _recipeServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public void OnInitializedAsync_WithNonZeroId_LoadsRecipe()
        {
            // Arrange
            ArrangeLookups();

            var existing = new RecipeEditModel { Id = 5, Name = "Loaded" };

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            // Act
            var cut = RenderComponent("5");

            // Assert
            Assert.That(cut.Instance.Recipe, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(cut.Instance.Recipe!.Id, Is.EqualTo(5));
                Assert.That(cut.Instance.Recipe!.Name, Is.EqualTo("Loaded"));
            });

            _recipeServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
        }

        [Test]
        public void OnInitializedAsync_WithNonZeroId_NullFromService_FallsBackToRecipeWithId()
        {
            // Arrange
            ArrangeLookups();

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync((RecipeEditModel?)null);

            // Act
            var cut = RenderComponent("5");

            // Assert
            Assert.That(cut.Instance.Recipe, Is.Not.Null);
            Assert.That(cut.Instance.Recipe!.Id, Is.EqualTo(5));
            _recipeServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
        }

        // ---------- SaveCoreAsync ----------
        [Test]
        public async Task SaveCoreAsync_AddsRecipe_WhenIdIsZero()
        {
            // Arrange
            ArrangeLookups();

            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            _recipeServiceMock
                .Setup(s => s.AddAsync(It.IsAny<RecipeEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent("0");

            var recipe = new RecipeEditModel { Id = 0, Name = "New Recipe" };

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
                s => s.AddAsync(It.Is<RecipeEditModel>(r => r.Name == "New Recipe")),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been saved successfully"),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/recipesoverview"));
        }

        [Test]
        public async Task SaveCoreAsync_UpdatesRecipe_WhenIdIsNonZero()
        {
            // Arrange
            ArrangeLookups();

            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            var existing = new RecipeEditModel { Id = 5, Name = "Loaded" };

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _recipeServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<RecipeEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent("5");

            var recipe = new RecipeEditModel { Id = 5, Name = "Updated" };

            var method = typeof(RecipeEdit) .GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [recipe])!;
                await task;
            });

            // Assert
            _recipeServiceMock.Verify(s => s.GetEditAsync(5), Times.Once); 
            _recipeServiceMock.Verify(
                s => s.UpdateAsync(It.Is<RecipeEditModel>(r => r.Id == 5)),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been saved successfully"),
                Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_ShowsGenericError_WhenResponseIsNull()
        {
            // Arrange
            ArrangeLookups();

            _recipeServiceMock
                .Setup(s => s.AddAsync(It.IsAny<RecipeEditModel>()))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent("0");

            var recipe = new RecipeEditModel { Id = 0, Name = "New Recipe" };

            var method = typeof(RecipeEdit) .GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [recipe])!;
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

            _recipeServiceMock
                .Setup(s => s.AddAsync(It.IsAny<RecipeEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent("0");

            var recipe = new RecipeEditModel { Id = 0, Name = "New Recipe" };

            var method = typeof(RecipeEdit) .GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [recipe])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowError("Validation error"),
                Times.Once);
        }

        // ---------- DeleteAsync / DeleteCoreAsync ----------
        [Test]
        public async Task DeleteAsync_DoesNothing_WhenRecipeIsNullOrIdZero()
        {
            // Arrange
            ArrangeLookups();
            var cut = RenderComponent("0");
            cut.Instance.Recipe = new RecipeEditModel { Id = 0 };

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

            var existing = new RecipeEditModel { Id = 5, Name = "Loaded" };

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _recipeServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync(response);

            var cut = RenderComponent("5");

            var recipe = new RecipeEditModel { Id = 5, Name = "ToDelete" };

            var method = typeof(RecipeEdit)  .GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [recipe])!;
                await task;
            });

            // Assert
            _recipeServiceMock.Verify(s => s.GetEditAsync(5), Times.Once); 
            _recipeServiceMock.Verify(s => s.DeleteAsync(5), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been deleted successfully"),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/recipesoverview"));
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            ArrangeLookups();

            var existing = new RecipeEditModel { Id = 5, Name = "Loaded" };

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _recipeServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent("5");

            var recipe = new RecipeEditModel { Id = 5 };

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
            _recipeServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
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

            var existing = new RecipeEditModel { Id = 5, Name = "Loaded" };

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _recipeServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync(response);

            var cut = RenderComponent("5");

            var recipe = new RecipeEditModel { Id = 5 };

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
            _recipeServiceMock.Verify(s => s.GetEditAsync(5), Times.Once); 
            _messageComponentMock.Verify(
                m => m.ShowError("Delete failed because of dependency"),
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
    }
}