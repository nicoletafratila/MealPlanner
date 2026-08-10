using System.Collections.ObjectModel;
using Common.Constants.Units;
using Common.Models;
using Common.Pagination;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Mobile.Services;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;
using RecipeBook.Shared.Resources;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.RecipeBook
{
    [TestFixture]
    public class RecipeEditViewModelTests
    {
        // Literal copy of the internal RecipeEditPage.AllCategoriesOption resx value used by the
        // production code. The resx-generated class is `internal` in MealPlanner.UI.Mobile and this
        // test project has no InternalsVisibleTo, so the exact literal is asserted instead.
        private const string AllCategoriesOption = "All categories";

        private Mock<IRecipeService> _recipeServiceMock = null!;
        private Mock<IRecipeCategoryService> _categoryServiceMock = null!;
        private Mock<IUnitService> _unitServiceMock = null!;
        private Mock<IProductService> _productServiceMock = null!;
        private Mock<IProductCategoryService> _productCategoryServiceMock = null!;
        private Mock<IShopService> _shopServiceMock = null!;
        private Mock<IRecipeService> _lookupRecipeServiceMock = null!;
        private RecipeEditViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _recipeServiceMock = new Mock<IRecipeService>(MockBehavior.Strict);
            _categoryServiceMock = new Mock<IRecipeCategoryService>(MockBehavior.Strict);
            _unitServiceMock = new Mock<IUnitService>(MockBehavior.Strict);
            _productServiceMock = new Mock<IProductService>(MockBehavior.Strict);
            _productCategoryServiceMock = new Mock<IProductCategoryService>(MockBehavior.Strict);
            _shopServiceMock = new Mock<IShopService>(MockBehavior.Strict);
            _lookupRecipeServiceMock = new Mock<IRecipeService>(MockBehavior.Strict);

            var lookupDataService = new ReferenceDataCacheService(
                _categoryServiceMock.Object,
                _unitServiceMock.Object,
                _productServiceMock.Object,
                _productCategoryServiceMock.Object,
                _shopServiceMock.Object,
                _lookupRecipeServiceMock.Object);

            _viewModel = new RecipeEditViewModel(_recipeServiceMock.Object, lookupDataService);
        }

        private void SetupLookups(
            List<RecipeCategoryModel> categories,
            List<UnitModel> units,
            List<ProductModel> products,
            List<ProductCategoryModel> productCategories)
        {
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<RecipeCategoryModel>>(p => p.PageSize == 100), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>(categories, Metadata.Create(1, 100, categories.Count)));

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<UnitModel>>(p => p.PageSize == 100), CancellationToken.None))
                .ReturnsAsync(new PagedList<UnitModel>(units, Metadata.Create(1, 100, units.Count)));

            _productServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<ProductModel>>(p => p.PageSize == 500), CancellationToken.None, true))
                .ReturnsAsync(new PagedList<ProductModel>(products, Metadata.Create(1, 500, products.Count)));

            _productCategoryServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<ProductCategoryModel>>(p => p.PageSize == 200), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductCategoryModel>(productCategories, Metadata.Create(1, 200, productCategories.Count)));

            _shopServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ShopModel>([], Metadata.Create(1, 200, 0)));
        }

        private static void ApplyRecipeId(RecipeEditViewModel viewModel, string id) =>
            viewModel.ApplyQueryAttributes(new Dictionary<string, object> { ["id"] = id });

        [Test]
        public void OnRecipeIdChanged_EmptyGuid_LoadsLookupsOnlyAndMarksNew()
        {
            var categories = new List<RecipeCategoryModel> { new(Guid.NewGuid(), "Desert") };
            var units = new List<UnitModel> { new(Guid.NewGuid(), "Kg", UnitType.Weight) };
            var products = new List<ProductModel> { new(Guid.NewGuid(), "Milk") };
            var productCategories = new List<ProductCategoryModel> { new(Guid.NewGuid(), "Dairy") };
            SetupLookups(categories, units, products, productCategories);

            ApplyRecipeId(_viewModel, Guid.Empty.ToString());

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsNew, Is.True);
                Assert.That(_viewModel.Categories, Has.Count.EqualTo(1));
                Assert.That(_viewModel.Units, Has.Count.EqualTo(1));
                Assert.That(_viewModel.Products, Has.Count.EqualTo(1));
                Assert.That(_viewModel.ProductCategories, Has.Count.EqualTo(2));
                Assert.That(_viewModel.ProductCategories[0].Id, Is.EqualTo(Guid.Empty));
                Assert.That(_viewModel.ProductCategories[0].Name, Is.EqualTo(AllCategoriesOption));
                Assert.That(_viewModel.ProductCategories[1], Is.SameAs(productCategories[0]));
                Assert.That(_viewModel.RecipeIngredients, Is.Empty);
                Assert.That(_viewModel.RecipeImage, Is.Null);
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _recipeServiceMock.Verify(s => s.GetEditAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void LoadAsync_ExistingRecipe_LoadsModelSetsCategoryAndSortsIngredientsByProductCategory()
        {
            var id = Guid.NewGuid();
            var categoryA = new RecipeCategoryModel(Guid.NewGuid(), "Desert");
            var categoryB = new RecipeCategoryModel(Guid.NewGuid(), "Main");
            var pcA = new ProductCategoryModel(Guid.NewGuid(), "Dairy");
            var pcB = new ProductCategoryModel(Guid.NewGuid(), "Bakery");
            var productInB = new ProductModel(Guid.NewGuid(), "Bread") { ProductCategory = pcB };
            var productInA = new ProductModel(Guid.NewGuid(), "Milk") { ProductCategory = pcA };
            var productNoCategory = new ProductModel(Guid.NewGuid(), "Salt");

            SetupLookups(
                [categoryA, categoryB],
                [],
                [productInB, productInA, productNoCategory],
                [pcA, pcB]);

            var ingredientInB = new RecipeIngredientEditModel { Product = productInB, ProductId = productInB.Id, Quantity = 1 };
            var ingredientInA = new RecipeIngredientEditModel { Product = productInA, ProductId = productInA.Id, Quantity = 2 };
            var ingredientUnmatched = new RecipeIngredientEditModel { Product = productNoCategory, ProductId = productNoCategory.Id, Quantity = 3 };

            var recipeModel = new RecipeEditModel(id, "Cake", categoryA.Id)
            {
                ImageContent = null,
                Ingredients = [ingredientInB, ingredientInA, ingredientUnmatched]
            };

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(recipeModel);

            ApplyRecipeId(_viewModel, id.ToString());

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsNew, Is.False);
                Assert.That(_viewModel.Model, Is.SameAs(recipeModel));
                Assert.That(_viewModel.SelectedCategory, Is.SameAs(categoryA));
                Assert.That(_viewModel.RecipeImage, Is.Null);
                Assert.That(_viewModel.RecipeIngredients, Has.Count.EqualTo(3));
                // ProductCategories = [All categories, pcA, pcB] => pcA-linked ingredient sorts before pcB-linked,
                // and the ingredient with no product category sorts last.
                Assert.That(_viewModel.RecipeIngredients[0], Is.SameAs(ingredientInA));
                Assert.That(_viewModel.RecipeIngredients[1], Is.SameAs(ingredientInB));
                Assert.That(_viewModel.RecipeIngredients[2], Is.SameAs(ingredientUnmatched));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public void LoadAsync_LookupServiceThrows_SetsErrorMessage()
        {
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>(), CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));
            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<UnitModel>([], Metadata.Create(1, 100, 0)));
            _productServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductModel>>(), CancellationToken.None, true))
                .ReturnsAsync(new PagedList<ProductModel>([], Metadata.Create(1, 500, 0)));
            _productCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], Metadata.Create(1, 200, 0)));
            _shopServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ShopModel>([], Metadata.Create(1, 200, 0)));

            ApplyRecipeId(_viewModel, Guid.Empty.ToString());

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("boom"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public void OnSelectedProductCategoryChanged_NonEmptyCategory_FiltersProductsAndResetsSelectedProduct()
        {
            var categoryA = new ProductCategoryModel(Guid.NewGuid(), "Dairy");
            var categoryB = new ProductCategoryModel(Guid.NewGuid(), "Bakery");
            var p1 = new ProductModel(Guid.NewGuid(), "Milk") { ProductCategory = categoryA };
            var p2 = new ProductModel(Guid.NewGuid(), "Bread") { ProductCategory = categoryB };
            var p3 = new ProductModel(Guid.NewGuid(), "Cheese") { ProductCategory = categoryA };
            _viewModel.Products = new ObservableCollection<ProductModel> { p1, p2, p3 };
            _viewModel.SelectedProduct = p1;

            _viewModel.SelectedProductCategory = categoryA;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ProductsByCategory, Is.EquivalentTo(new[] { p1, p3 }));
                Assert.That(_viewModel.SelectedProduct, Is.Null);
            }
        }

        [Test]
        public void OnSelectedProductCategoryChanged_SetBackToNull_ShowsAllProducts()
        {
            var categoryA = new ProductCategoryModel(Guid.NewGuid(), "Dairy");
            var p1 = new ProductModel(Guid.NewGuid(), "Milk") { ProductCategory = categoryA };
            var p2 = new ProductModel(Guid.NewGuid(), "Bread");
            _viewModel.Products = new ObservableCollection<ProductModel> { p1, p2 };
            _viewModel.SelectedProductCategory = categoryA;

            _viewModel.SelectedProductCategory = null;

            Assert.That(_viewModel.ProductsByCategory, Is.EquivalentTo(new[] { p1, p2 }));
        }

        [Test]
        public void OnSelectedProductCategoryChanged_SyntheticAllCategoriesEntry_ShowsAllProducts()
        {
            var categoryA = new ProductCategoryModel(Guid.NewGuid(), "Dairy");
            var p1 = new ProductModel(Guid.NewGuid(), "Milk") { ProductCategory = categoryA };
            var p2 = new ProductModel(Guid.NewGuid(), "Bread");
            _viewModel.Products = new ObservableCollection<ProductModel> { p1, p2 };
            _viewModel.SelectedProductCategory = categoryA;

            _viewModel.SelectedProductCategory = new ProductCategoryModel(Guid.Empty, AllCategoriesOption);

            Assert.That(_viewModel.ProductsByCategory, Is.EquivalentTo(new[] { p1, p2 }));
        }

        [Test]
        public void OnSelectedProductChanged_ProductWithBaseUnit_FiltersUnitsByTypeAndSelectsMatchingUnit()
        {
            var kg = new UnitModel(Guid.NewGuid(), "Kilogram", UnitType.Weight);
            var gram = new UnitModel(Guid.NewGuid(), "Gram", UnitType.Weight);
            var liter = new UnitModel(Guid.NewGuid(), "Liter", UnitType.Liquid);
            _viewModel.Units = new ObservableCollection<UnitModel> { kg, gram, liter };
            _viewModel.QuantityText = "5";

            var product = new ProductModel(Guid.NewGuid(), "Milk") { BaseUnit = kg };
            _viewModel.SelectedProduct = product;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.UnitsForProduct, Is.EquivalentTo(new[] { kg, gram }));
                Assert.That(_viewModel.SelectedUnit, Is.SameAs(kg));
                Assert.That(_viewModel.QuantityText, Is.Empty);
            }
        }

        [Test]
        public void OnSelectedProductChanged_ProductWithoutBaseUnit_ShowsAllUnitsAndNoSelectedUnit()
        {
            var kg = new UnitModel(Guid.NewGuid(), "Kilogram", UnitType.Weight);
            var liter = new UnitModel(Guid.NewGuid(), "Liter", UnitType.Liquid);
            _viewModel.Units = new ObservableCollection<UnitModel> { kg, liter };

            var product = new ProductModel(Guid.NewGuid(), "Mystery item");
            _viewModel.SelectedProduct = product;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.UnitsForProduct, Is.EquivalentTo(new[] { kg, liter }));
                Assert.That(_viewModel.SelectedUnit, Is.Null);
            }
        }

        [Test]
        public void OnSelectedProductChanged_SetBackToNull_ShowsAllUnitsAndNoSelectedUnit()
        {
            var kg = new UnitModel(Guid.NewGuid(), "Kilogram", UnitType.Weight);
            var liter = new UnitModel(Guid.NewGuid(), "Liter", UnitType.Liquid);
            _viewModel.Units = new ObservableCollection<UnitModel> { kg, liter };
            _viewModel.SelectedProduct = new ProductModel(Guid.NewGuid(), "Milk") { BaseUnit = kg };

            _viewModel.SelectedProduct = null;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.UnitsForProduct, Is.EquivalentTo(new[] { kg, liter }));
                Assert.That(_viewModel.SelectedUnit, Is.Null);
            }
        }

        [Test]
        public void AddIngredient_NoSelectedProduct_DoesNothing()
        {
            _viewModel.SelectedProduct = null;
            _viewModel.SelectedUnit = new UnitModel(Guid.NewGuid(), "Kg", UnitType.Weight);
            _viewModel.QuantityText = "2";

            _viewModel.AddIngredientCommand.Execute(null);

            Assert.That(_viewModel.RecipeIngredients, Is.Empty);
        }

        [Test]
        public void AddIngredient_NoSelectedUnit_DoesNothing()
        {
            _viewModel.Units = new ObservableCollection<UnitModel>();
            _viewModel.SelectedProduct = new ProductModel(Guid.NewGuid(), "Milk");
            _viewModel.SelectedUnit = null;
            _viewModel.QuantityText = "2";

            _viewModel.AddIngredientCommand.Execute(null);

            Assert.That(_viewModel.RecipeIngredients, Is.Empty);
        }

        [TestCase("abc")]
        [TestCase("0")]
        [TestCase("-1")]
        [TestCase("")]
        public void AddIngredient_InvalidQuantity_DoesNothing(string quantityText)
        {
            var unit = new UnitModel(Guid.NewGuid(), "Kg", UnitType.Weight);
            _viewModel.Units = new ObservableCollection<UnitModel> { unit };
            _viewModel.SelectedProduct = new ProductModel(Guid.NewGuid(), "Milk");
            _viewModel.SelectedUnit = unit;
            _viewModel.QuantityText = quantityText;

            _viewModel.AddIngredientCommand.Execute(null);

            Assert.That(_viewModel.RecipeIngredients, Is.Empty);
        }

        [Test]
        public void AddIngredient_NewProduct_AddsIngredientAndResetsSelection()
        {
            _viewModel.Model.Id = Guid.NewGuid();
            var unit = new UnitModel(Guid.NewGuid(), "Kg", UnitType.Weight);
            var product = new ProductModel(Guid.NewGuid(), "Milk");
            _viewModel.Units = new ObservableCollection<UnitModel> { unit };
            _viewModel.SelectedProduct = product;
            _viewModel.SelectedUnit = unit;
            _viewModel.QuantityText = "2.5";

            _viewModel.AddIngredientCommand.Execute(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.RecipeIngredients, Has.Count.EqualTo(1));
                var added = _viewModel.RecipeIngredients[0];
                Assert.That(added.RecipeId, Is.EqualTo(_viewModel.Model.Id));
                Assert.That(added.ProductId, Is.EqualTo(product.Id));
                Assert.That(added.Product, Is.SameAs(product));
                Assert.That(added.UnitId, Is.EqualTo(unit.Id));
                Assert.That(added.Unit, Is.SameAs(unit));
                Assert.That(added.Quantity, Is.EqualTo(2.5m));
                Assert.That(_viewModel.QuantityText, Is.Empty);
                Assert.That(_viewModel.SelectedProduct, Is.Null);
            }
        }

        [Test]
        public void AddIngredient_SameProductAddedTwice_MergesQuantityInsteadOfDuplicating()
        {
            var unit = new UnitModel(Guid.NewGuid(), "Liter", UnitType.Liquid);
            var product = new ProductModel(Guid.NewGuid(), "Milk");
            _viewModel.Units = new ObservableCollection<UnitModel> { unit };

            _viewModel.SelectedProduct = product;
            _viewModel.SelectedUnit = unit;
            _viewModel.QuantityText = "2";
            _viewModel.AddIngredientCommand.Execute(null);

            _viewModel.SelectedProduct = product;
            _viewModel.SelectedUnit = unit;
            _viewModel.QuantityText = "3";
            _viewModel.AddIngredientCommand.Execute(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.RecipeIngredients, Has.Count.EqualTo(1));
                Assert.That(_viewModel.RecipeIngredients[0].Quantity, Is.EqualTo(5m));
                Assert.That(_viewModel.RecipeIngredients[0].Product, Is.SameAs(product));
            }
        }

        [Test]
        public void AddIngredient_TwoDifferentProducts_AccumulatesBothInList()
        {
            var unit = new UnitModel(Guid.NewGuid(), "Kg", UnitType.Weight);
            var productA = new ProductModel(Guid.NewGuid(), "Milk");
            var productB = new ProductModel(Guid.NewGuid(), "Bread");
            _viewModel.Units = new ObservableCollection<UnitModel> { unit };

            _viewModel.SelectedProduct = productA;
            _viewModel.SelectedUnit = unit;
            _viewModel.QuantityText = "1";
            _viewModel.AddIngredientCommand.Execute(null);

            _viewModel.SelectedProduct = productB;
            _viewModel.SelectedUnit = unit;
            _viewModel.QuantityText = "2";
            _viewModel.AddIngredientCommand.Execute(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.RecipeIngredients, Has.Count.EqualTo(2));
                Assert.That(_viewModel.RecipeIngredients.Select(i => i.Product), Is.EquivalentTo(new[] { productA, productB }));
            }
        }

        [Test]
        public void ApplyQueryAttributes_ReappliedForSameNewRecipe_DoesNotClearAddedIngredients()
        {
            // Shell re-invokes ApplyQueryAttributes on the same page instance when the product
            // selector popup is shown. It must not wipe ingredients added since the page loaded.
            var categories = new List<RecipeCategoryModel> { new(Guid.NewGuid(), "Desert") };
            var units = new List<UnitModel> { new(Guid.NewGuid(), "Kg", UnitType.Weight) };
            var product = new ProductModel(Guid.NewGuid(), "Milk");
            var products = new List<ProductModel> { product };
            var productCategories = new List<ProductCategoryModel> { new(Guid.NewGuid(), "Dairy") };
            SetupLookups(categories, units, products, productCategories);

            ApplyRecipeId(_viewModel, Guid.Empty.ToString());

            _viewModel.SelectedProduct = product;
            _viewModel.SelectedUnit = units[0];
            _viewModel.QuantityText = "1";
            _viewModel.AddIngredientCommand.Execute(null);
            Assert.That(_viewModel.RecipeIngredients, Has.Count.EqualTo(1));

            ApplyRecipeId(_viewModel, Guid.Empty.ToString());

            Assert.That(_viewModel.RecipeIngredients, Has.Count.EqualTo(1));
        }

        [Test]
        public void RemoveIngredient_RemovesGivenIngredientFromCollection()
        {
            var ingredient = new RecipeIngredientEditModel { Product = new ProductModel(Guid.NewGuid(), "Milk"), Quantity = 1 };
            _viewModel.RecipeIngredients = new ObservableCollection<RecipeIngredientEditModel> { ingredient };

            _viewModel.RemoveIngredientCommand.Execute(ingredient);

            Assert.That(_viewModel.RecipeIngredients, Is.Empty);
        }

        private void SetValidSaveState()
        {
            _viewModel.Model.Name = "Cake";
            _viewModel.Model.ImageContent = [1, 2, 3];
            _viewModel.SelectedCategory = new RecipeCategoryModel(Guid.NewGuid(), "Desert");
            _viewModel.RecipeIngredients = new ObservableCollection<RecipeIngredientEditModel>
            {
                new() { Product = new ProductModel(Guid.NewGuid(), "Milk"), Quantity = 1 }
            };
        }

        [Test]
        public async Task SaveAsync_NameMissing_SetsErrorAndDoesNotCallService()
        {
            SetValidSaveState();
            _viewModel.Model.Name = string.Empty;

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(RecipeBookSharedMessages.RecipeNameRequired));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _recipeServiceMock.Verify(s => s.AddAsync(It.IsAny<RecipeEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
            _recipeServiceMock.Verify(s => s.UpdateAsync(It.IsAny<RecipeEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SaveAsync_ImageMissing_SetsErrorAndDoesNotCallService()
        {
            SetValidSaveState();
            _viewModel.Model.ImageContent = null;

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(RecipeBookSharedMessages.ImageRequired));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _recipeServiceMock.Verify(s => s.AddAsync(It.IsAny<RecipeEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SaveAsync_CategoryMissing_SetsErrorAndDoesNotCallService()
        {
            SetValidSaveState();
            _viewModel.SelectedCategory = null;

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(RecipeBookSharedMessages.RecipeCategoryRequired));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _recipeServiceMock.Verify(s => s.AddAsync(It.IsAny<RecipeEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SaveAsync_NoIngredients_SetsErrorAndDoesNotCallService()
        {
            SetValidSaveState();
            _viewModel.RecipeIngredients = new ObservableCollection<RecipeIngredientEditModel>();

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(RecipeBookSharedMessages.RecipeRequiresIngredients));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _recipeServiceMock.Verify(s => s.AddAsync(It.IsAny<RecipeEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SaveAsync_IngredientQuantityNotPositive_SetsErrorAndDoesNotCallService()
        {
            SetValidSaveState();
            _viewModel.RecipeIngredients = new ObservableCollection<RecipeIngredientEditModel>
            {
                new() { Product = new ProductModel(Guid.NewGuid(), "Milk"), Quantity = 0 }
            };

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(RecipeBookSharedMessages.IngredientQuantityPositive));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _recipeServiceMock.Verify(s => s.AddAsync(It.IsAny<RecipeEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SaveAsync_WhenBusy_DoesNotValidateOrCallService()
        {
            SetValidSaveState();
            _viewModel.IsBusy = true;

            await _viewModel.SaveCommand.ExecuteAsync(null);

            _recipeServiceMock.Verify(s => s.AddAsync(It.IsAny<RecipeEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
            _recipeServiceMock.Verify(s => s.UpdateAsync(It.IsAny<RecipeEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SaveAsync_NewRecipeValid_CallsAddAsyncWithCategoryAndIngredientsSet()
        {
            SetValidSaveState();
            _viewModel.IsNew = true;

            RecipeEditModel? captured = null;
            _recipeServiceMock
                .Setup(s => s.AddAsync(It.IsAny<RecipeEditModel>(), CancellationToken.None))
                .Callback<RecipeEditModel, CancellationToken>((m, _) => captured = m)
                .ReturnsAsync(CommandResponse.Success());

            // Shell.Current.GoToAsync runs after a successful save, inside the try/catch, so the
            // resulting NullReferenceException in this test host is swallowed into ErrorMessage.
            // Only the service call and the model passed to it are verified here.
            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(captured, Is.Not.Null);
                Assert.That(captured!.RecipeCategoryId, Is.EqualTo(_viewModel.SelectedCategory!.Id));
                Assert.That(captured.Ingredients, Is.EquivalentTo(_viewModel.RecipeIngredients));
            }
            _recipeServiceMock.Verify(s => s.UpdateAsync(It.IsAny<RecipeEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SaveAsync_ExistingRecipeValid_CallsUpdateAsync()
        {
            SetValidSaveState();
            _viewModel.IsNew = false;

            _recipeServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<RecipeEditModel>(), CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.SaveCommand.ExecuteAsync(null);

            _recipeServiceMock.Verify(s => s.UpdateAsync(_viewModel.Model, CancellationToken.None), Times.Once);
            _recipeServiceMock.Verify(s => s.AddAsync(It.IsAny<RecipeEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SaveAsync_ServiceReturnsFailure_SetsErrorFromResponseMessage()
        {
            SetValidSaveState();
            _viewModel.IsNew = true;

            _recipeServiceMock
                .Setup(s => s.AddAsync(It.IsAny<RecipeEditModel>(), CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("save rejected"));

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("save rejected"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task SaveAsync_ServiceThrows_SetsErrorMessage()
        {
            SetValidSaveState();
            _viewModel.IsNew = true;

            _recipeServiceMock
                .Setup(s => s.AddAsync(It.IsAny<RecipeEditModel>(), CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("boom"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }
    }
}
