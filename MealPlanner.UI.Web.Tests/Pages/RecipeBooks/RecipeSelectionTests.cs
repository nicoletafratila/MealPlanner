using System.Reflection;
using BlazorBootstrap;
using Blazored.Modal;
using Bunit;
using Common.Pagination;
using MealPlanner.UI.Web.Pages;
using MealPlanner.UI.Web.Pages.RecipeBooks;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Tests.Pages.RecipeBooks
{
    [TestFixture]
    public class RecipeSelectionTests
    {
        private BunitContext _ctx = null!;
        private Mock<IRecipeCategoryService> _categoryServiceMock = null!;
        private Mock<IRecipeService> _recipeServiceMock = null!;
        private Mock<IModalController> _modalControllerMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _categoryServiceMock = new Mock<IRecipeCategoryService>(MockBehavior.Strict);
            _recipeServiceMock = new Mock<IRecipeService>(MockBehavior.Strict);
            _modalControllerMock = new Mock<IModalController>(MockBehavior.Strict);

            _ctx.Services.AddSingleton(_categoryServiceMock.Object);
            _ctx.Services.AddSingleton(_recipeServiceMock.Object);
            _ctx.Services.AddBlazoredModal();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
            _categoryServiceMock.Reset();
            _recipeServiceMock.Reset();
            _modalControllerMock.Reset();
        }

        private IRenderedComponent<RecipeSelection> RenderComponent()
        {
            // Just render the component directly
            return _ctx.Render<RecipeSelection>();
        }

        // ---------- OnInitializedAsync ----------
        [Test]
        public void OnInitializedAsync_LoadsCategories()
        {
            // Arrange
            var categoryItems = new List<RecipeCategoryModel>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };

            var categories = new PagedList<RecipeCategoryModel>(
                categoryItems, new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 2 });

            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
                .ReturnsAsync(categories);

            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>()))
                .ReturnsAsync(new PagedList<RecipeModel>(new List<RecipeModel>(), new Metadata()));

            // Act
            var cut = RenderComponent();
            cut.Instance.ModalController = _modalControllerMock.Object;

            // Assert
            Assert.That(cut.Instance.Categories, Is.Not.Null);
            Assert.That(cut.Instance.Categories!.Items, Has.Count.EqualTo(2));

            _categoryServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()),
                Times.Once);
        }

        // ---------- OnRecipeCategoryChangedAsync ----------
        [Test]
        public async Task OnRecipeCategoryChangedAsync_WithCategoryId_LoadsRecipes_WithCorrectFilterAndSorting()
        {
            // Arrange
            var categoryItems = new List<RecipeCategoryModel> { new() { Id = 1 } };
            var categories = new PagedList<RecipeCategoryModel>(
                categoryItems, new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });

            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
                .ReturnsAsync(categories);

            var recipes = new List<RecipeModel>
            {
                new() { Id = 10, Name = "A" },
                new() { Id = 11, Name = "B" }
            };

            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>()))
                .ReturnsAsync(new PagedList<RecipeModel>(
                    recipes, new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 2 }));

            var cut = RenderComponent();
            cut.Instance.ModalController = _modalControllerMock.Object;

            var method = typeof(RecipeSelection).GetMethod("OnRecipeCategoryChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var args = new ChangeEventArgs { Value = "1" };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { args })!;
                await task;
            });

            // Assert that SearchAsync was called with the expected filter & sorting
            _recipeServiceMock.Verify(
                s => s.SearchAsync(It.Is<QueryParameters<RecipeModel>>(qp =>
                    qp.Filters != null &&
                    qp.Filters.Count() == 1 &&
                    qp.Filters.First().PropertyName == "RecipeCategoryId" &&
                    (string)qp.Filters.First().Value == "1" &&
                    qp.Sorting != null &&
                    qp.Sorting.Count() == 1 &&
                    qp.Sorting.First().PropertyName == "Name" &&
                    qp.Sorting.First().Direction == SortDirection.Ascending &&
                    qp.PageSize == int.MaxValue &&
                    qp.PageNumber == 1)),
                Times.Once);

            Assert.That(cut.Instance.Recipes, Is.Not.Null);
            Assert.That(cut.Instance.Recipes!.Items, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task OnRecipeCategoryChangedAsync_WithEmptySelection_LoadsRecipes_WithNoFilter()
        {
            // Arrange
            var categories = new PagedList<RecipeCategoryModel>(
                new List<RecipeCategoryModel>(), new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 0 });

            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
                .ReturnsAsync(categories);

            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>()))
                .ReturnsAsync(new PagedList<RecipeModel>([], new Metadata()));

            var cut = RenderComponent();
            cut.Instance.ModalController = _modalControllerMock.Object;

            var method = typeof(RecipeSelection).GetMethod("OnRecipeCategoryChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var args = new ChangeEventArgs { Value = "" };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { args })!;
                await task;
            });

            // Assert: filters empty, sorting by Name ascending
            _recipeServiceMock.Verify(
                s => s.SearchAsync(It.Is<QueryParameters<RecipeModel>>(qp =>
                    qp.Filters != null &&
                    qp.Filters.Count() == 0 &&
                    qp.Sorting != null &&
                    qp.Sorting.Count() == 1 &&
                    qp.Sorting.First().PropertyName == "Name" &&
                    qp.Sorting.First().Direction == SortDirection.Ascending)),
                Times.Once);
        }

        // ---------- SaveAsync / CancelAsync using IModalController mock ----------
        [Test]
        public async Task SaveAsync_UsesModalController_WithRecipeId()
        {
            // Arrange
            var categories = new PagedList<RecipeCategoryModel>(
                new List<RecipeCategoryModel>(), new Metadata());

            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
                .ReturnsAsync(categories);

            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>()))
                .ReturnsAsync(new PagedList<RecipeModel>(
                    new List<RecipeModel>(), new Metadata()));

            _modalControllerMock
                .Setup(m => m.CloseAsync(It.IsAny<object?>()))
                .Returns(Task.CompletedTask);

            var cut = RenderComponent();
            cut.Instance.ModalController = _modalControllerMock.Object;

            // Set RecipeId via reflection
            typeof(RecipeSelection)
                .GetProperty("RecipeId", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!
                .SetValue(cut.Instance, "42");

            var method = typeof(RecipeSelection).GetMethod("SaveAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, Array.Empty<object>())!;
                await task;
            });

            // Assert modal controller was called with the RecipeId
            _modalControllerMock.Verify(
                m => m.CloseAsync("42"),
                Times.Once);
        }

        [Test]
        public async Task CancelAsync_UsesModalController_Cancel()
        {
            // Arrange
            var categories = new PagedList<RecipeCategoryModel>(
                new List<RecipeCategoryModel>(), new Metadata());

            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
                .ReturnsAsync(categories);

            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>()))
                .ReturnsAsync(new PagedList<RecipeModel>(
                    new List<RecipeModel>(), new Metadata()));

            _modalControllerMock
                .Setup(m => m.CancelAsync())
                .Returns(Task.CompletedTask);

            var cut = RenderComponent();
            cut.Instance.ModalController = _modalControllerMock.Object;

            var method = typeof(RecipeSelection).GetMethod("CancelAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, Array.Empty<object>())!;
                await task;
            });

            // Assert
            _modalControllerMock.Verify(m => m.CancelAsync(), Times.Once);
        }
    }
}

