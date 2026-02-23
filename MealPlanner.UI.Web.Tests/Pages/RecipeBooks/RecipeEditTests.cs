using System.Reflection;
using Bunit;
using Common.Pagination;
using Common.UI;
using MealPlanner.UI.Web.Pages.RecipeBooks;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
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

            _ctx.Services.AddBlazorBootstrap();

            _ctx.Services.AddSingleton(_recipeServiceMock.Object);
            _ctx.Services.AddSingleton(_recipeCategoryServiceMock.Object);
            _ctx.Services.AddSingleton(_productCategoryServiceMock.Object);
            _ctx.Services.AddSingleton(_productServiceMock.Object);
            _ctx.Services.AddSingleton(_unitServiceMock.Object);
            _ctx.Services.AddSingleton(_messageComponentMock.Object);
            _ctx.Services.AddLogging();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
        }

        private IRenderedComponent<RecipeEdit> RenderComponent(string? id = null)
        {
            return _ctx.Render<RecipeEdit>(parameters =>
            {
                if (id != null)
                {
                    parameters.Add(p => p.Id, id);
                }

                // Cascading MessageComponent
                parameters.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
            });
        }

        // ---------- OnInitializedAsync ----------
        [Test]
        public void OnInitializedAsync_WithIdZero_CreatesNewRecipe()
        {
            // Arrange
            _recipeCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([], new Metadata()));

            _productCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], new Metadata()));

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>()))
                .ReturnsAsync(new PagedList<UnitModel>([], new Metadata()));

            // Act
            var cut = RenderComponent(id: "0");

            // Assert
            Assert.That(cut.Instance.Recipe, Is.Not.Null);
            Assert.That(cut.Instance.Recipe!.Id, Is.EqualTo(0));

            _recipeCategoryServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()),
                Times.Once);

            _productCategoryServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()),
                Times.Once);

            _unitServiceMock.Verify(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>()), Times.Once);
        }

        [Test]
        public void OnInitializedAsync_WithNonZeroId_LoadsRecipe()
        {
            // Arrange
            _recipeCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([], new Metadata()));

            _productCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], new Metadata()));

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>()))
                .ReturnsAsync(new PagedList<UnitModel>([], new Metadata()));

            var existing = new RecipeEditModel { Id = 5, Name = "Loaded Recipe" };

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            // Act
            var cut = RenderComponent(id: "5");

            // Assert
            Assert.That(cut.Instance.Recipe, Is.Not.Null);
            Assert.That(cut.Instance.Recipe!.Id, Is.EqualTo(5));
            Assert.That(cut.Instance.Recipe!.Name, Is.EqualTo("Loaded Recipe"));

            _recipeServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
        }

        // ---------- CanAddIngredient ----------
        [Test]
        public void CanAddIngredient_False_WhenRequiredFieldsMissing()
        {
            // Arrange
            _recipeCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([], new Metadata()));

            _productCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], new Metadata()));

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>()))
                .ReturnsAsync(new PagedList<UnitModel>([], new Metadata()));

            var cut = RenderComponent("0");

            // Act
            cut.Instance.ProductId = "0";
            cut.Instance.UnitId = "1";
            cut.Instance.Quantity = "1";

            var canAddProp = typeof(RecipeEdit).GetProperty("CanAddIngredient", BindingFlags.Instance | BindingFlags.NonPublic);
            var result = (bool)canAddProp!.GetValue(cut.Instance)!;

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void CanAddIngredient_True_WhenAllFieldsValid()
        {
            // Arrange
            _recipeCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([], new Metadata()));

            _productCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], new Metadata()));

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>()))
                .ReturnsAsync(new PagedList<UnitModel>([], new Metadata()));

            var cut = RenderComponent("0");

            cut.Instance.ProductId = "10";
            cut.Instance.UnitId = "1";
            cut.Instance.Quantity = "2.5";

            var canAddProp = typeof(RecipeEdit).GetProperty("CanAddIngredient", BindingFlags.Instance | BindingFlags.NonPublic);
            var result = (bool)canAddProp!.GetValue(cut.Instance)!;

            Assert.That(result, Is.True);
        }

        // ---------- AddIngredient ----------
        [Test]
        public void AddIngredient_AddsNewIngredient_AndResetsFields()
        {
            // Arrange basic initialization
            _recipeCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([], new Metadata()));

            _productCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], new Metadata()));

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>()))
                .ReturnsAsync(new PagedList<UnitModel>([], new Metadata()));

            var products = new PagedList<ProductModel>(
                new List<ProductModel> { new() { Id = 5, Name = "Flour" } },
                new Metadata());

            var baseUnits = new PagedList<UnitModel>(
                new List<UnitModel> { new() { Id = 1, Name = "g" } },
                new Metadata());

            _productServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductModel>>()))
                .ReturnsAsync(products);

            // Act
            var cut = RenderComponent("0");
            cut.Instance.Recipe = new RecipeEditModel { Id = 1, Ingredients = new List<RecipeIngredientEditModel>() };
            cut.Instance.Products = products;
            cut.Instance.Units = baseUnits.Items;

            cut.Instance.ProductId = "5";
            cut.Instance.UnitId = "1";
            cut.Instance.Quantity = "100";

            var method = typeof(RecipeEdit).GetMethod("AddIngredient", BindingFlags.Instance | BindingFlags.NonPublic);
            method!.Invoke(cut.Instance, null);

            // Assert
            Assert.That(cut.Instance.Recipe!.Ingredients, Is.Not.Null);
            Assert.That(cut.Instance.Recipe!.Ingredients!, Has.Count.EqualTo(1));
            var ingredient = cut.Instance.Recipe!.Ingredients!.Single();
            Assert.Multiple(() =>
            {
                Assert.That(ingredient.Product!.Id, Is.EqualTo(5));
                Assert.That(ingredient.UnitId, Is.EqualTo(1));
                Assert.That(ingredient.Quantity, Is.EqualTo(100m));
                Assert.That(cut.Instance.Quantity, Is.EqualTo(string.Empty));
                Assert.That(cut.Instance.UnitId, Is.EqualTo(string.Empty));
            });
        }

        [Test]
        public void AddIngredient_SameProductDifferentUnit_ShowsError()
        {
            // Arrange
            _recipeCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([], new Metadata()));

            _productCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], new Metadata()));

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>()))
                .ReturnsAsync(new PagedList<UnitModel>([], new Metadata()));

            var cut = RenderComponent("0");

            cut.Instance.Recipe = new RecipeEditModel
            {
                Id = 1,
                Ingredients =
                [
                    new RecipeIngredientEditModel
                    {
                        Product = new ProductModel { Id = 5 },
                        Unit = new UnitModel { Id = 1 },
                        UnitId = 1,
                        Quantity = 50
                    }
                ]
            };

            cut.Instance.Products = new PagedList<ProductModel>(
                new List<ProductModel> { new() { Id = 5 } },
                new Metadata());

            cut.Instance.Units = new List<UnitModel>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };

            cut.Instance.ProductId = "5";
            cut.Instance.UnitId = "2"; 
            cut.Instance.Quantity = "25";

            var method = typeof(RecipeEdit).GetMethod("AddIngredient", BindingFlags.Instance | BindingFlags.NonPublic);
            method!.Invoke(cut.Instance, null);

            _messageComponentMock.Verify(
                m => m.ShowError("The same ingredient was added to the recipe with a different unit of measurement."),
                Times.Once);
        }

        // ---------- OnProductCategoryChangedAsync ----------
        [Test]
        public async Task OnProductCategoryChangedAsync_BuildsFilters_AndResetsFields()
        {
            // Arrange
            _recipeCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([], new Metadata()));

            _productCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], new Metadata()));

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>()))
                .ReturnsAsync(new PagedList<UnitModel>([], new Metadata()));

            var products = new PagedList<ProductModel>(
                new List<ProductModel> { new() { Id = 1 } },
                new Metadata());

            _productServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductModel>>()))
                .ReturnsAsync(products);

            var cut = RenderComponent("0");

            var method = typeof(RecipeEdit).GetMethod(
                "OnProductCategoryChangedAsync",
                BindingFlags.Instance | BindingFlags.NonPublic);

            var args = new ChangeEventArgs { Value = "3" };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { args })!;
                await task;
            });

            // Assert: verify filters built with ProductCategoryId == "3"
            _productServiceMock.Verify(
                s => s.SearchAsync(It.Is<QueryParameters<ProductModel>>(qp =>
                    qp.Filters != null &&
                    qp.Filters.Count() == 1 &&
                    qp.Filters.First().PropertyName == "ProductCategoryId" &&
                    (string)qp.Filters.First().Value == "3")),
                Times.Once);

            Assert.That(cut.Instance.ProductId, Is.EqualTo(string.Empty));
            Assert.That(cut.Instance.Quantity, Is.EqualTo(string.Empty));
        }

        // ---------- OnInputFileChangeAsync ----------
        [Test]
        public async Task OnInputFileChangeAsync_SetsImageContent_WhenWithinLimit()
        {
            // Arrange
            _recipeCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([], new Metadata()));

            _productCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], new Metadata()));

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>()))
                .ReturnsAsync(new PagedList<UnitModel>([], new Metadata()));

            var cut = RenderComponent("0");
            cut.Instance.Recipe = new RecipeEditModel { Id = 1 };

            var fileBytes = new byte[] { 1, 2, 3, 4 };
            var fakeFile = new FakeBrowserFile(fileBytes, "img.png", "image/png");
            var args = new InputFileChangeEventArgs(new[] { fakeFile });

            var method = typeof(RecipeEdit).GetMethod(
                "OnInputFileChangeAsync",
                BindingFlags.Instance | BindingFlags.NonPublic);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { args })!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.Recipe!.ImageContent, Is.Not.Null);
            Assert.That(cut.Instance.Recipe!.ImageContent!.SequenceEqual(fileBytes), Is.True);
        }

        [Test]
        public async Task OnInputFileChangeAsync_ShowsError_WhenFileTooLarge()
        {
            // Arrange
            _recipeCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([], new Metadata()));

            _productCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], new Metadata()));

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>()))
                .ReturnsAsync(new PagedList<UnitModel>([], new Metadata()));

            var cut = RenderComponent("0");
            cut.Instance.Recipe = new RecipeEditModel { Id = 1 };

            var bigBytes = new byte[1024 * 1024 * 5]; // 5 MB
            var fakeFile = new FakeBrowserFile(bigBytes, "big.bin", "application/octet-stream", throwOnOpen: true);
            var args = new InputFileChangeEventArgs(new[] { fakeFile });

            var method = typeof(RecipeEdit).GetMethod(
                "OnInputFileChangeAsync",
                BindingFlags.Instance | BindingFlags.NonPublic);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { args })!;
                await task;
            });

            // Assert: error shown
            _messageComponentMock.Verify(
                m => m.ShowError(It.Is<string>(msg => msg.Contains("Maximum allowed size"))),
                Times.Once);
        }
    }
}