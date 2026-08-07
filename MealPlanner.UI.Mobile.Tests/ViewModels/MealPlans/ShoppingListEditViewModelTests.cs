using System.Collections.ObjectModel;
using Common.Constants.Units;
using Common.Models;
using Common.Pagination;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.Shared.Resources;
using MealPlanner.UI.Mobile.Services;
using MealPlanner.UI.Mobile.ViewModels.MealPlans;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.MealPlans
{
    [TestFixture]
    public class ShoppingListEditViewModelTests
    {
        private Mock<IShoppingListService> _shoppingListServiceMock = null!;
        private Mock<IShopService> _shopServiceMock = null!;
        private Mock<IProductCategoryService> _productCategoryServiceMock = null!;
        private Mock<IProductService> _productServiceMock = null!;
        private Mock<IUnitService> _unitServiceMock = null!;
        private Mock<IMealPlanService> _mealPlanServiceMock = null!;
        private Mock<IRecipeService> _recipeServiceMock = null!;
        private Mock<IRecipeCategoryService> _lookupRecipeCategoryServiceMock = null!;
        private Mock<IShopService> _lookupShopServiceMock = null!;
        private ShoppingListEditViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _shoppingListServiceMock = new Mock<IShoppingListService>(MockBehavior.Strict);
            _shopServiceMock = new Mock<IShopService>(MockBehavior.Strict);
            _productCategoryServiceMock = new Mock<IProductCategoryService>(MockBehavior.Strict);
            _productServiceMock = new Mock<IProductService>(MockBehavior.Strict);
            _unitServiceMock = new Mock<IUnitService>(MockBehavior.Strict);
            _mealPlanServiceMock = new Mock<IMealPlanService>(MockBehavior.Strict);
            _recipeServiceMock = new Mock<IRecipeService>(MockBehavior.Strict);
            _lookupRecipeCategoryServiceMock = new Mock<IRecipeCategoryService>(MockBehavior.Strict);
            _lookupShopServiceMock = new Mock<IShopService>(MockBehavior.Strict);

            var lookupDataService = new ReferenceDataCacheService(
                _lookupRecipeCategoryServiceMock.Object,
                _unitServiceMock.Object,
                _productServiceMock.Object,
                _productCategoryServiceMock.Object,
                _lookupShopServiceMock.Object,
                _recipeServiceMock.Object);

            _viewModel = new ShoppingListEditViewModel(
                _shoppingListServiceMock.Object,
                _shopServiceMock.Object,
                _productServiceMock.Object,
                _mealPlanServiceMock.Object,
                _recipeServiceMock.Object,
                lookupDataService);
        }

        private void SetupLoadDependencies(
            IReadOnlyList<ShopModel>? shops = null,
            IReadOnlyList<ProductCategoryModel>? categories = null,
            IReadOnlyList<UnitModel>? units = null)
        {
            shops ??= [];
            categories ??= [];
            units ??= [];

            _lookupShopServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ShopModel>(shops, Metadata.Create(1, 200, shops.Count)));
            _productCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductCategoryModel>(categories, Metadata.Create(1, 200, categories.Count)));
            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<UnitModel>(units, Metadata.Create(1, 200, units.Count)));
            _lookupRecipeCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([], Metadata.Create(1, 100, 0)));
        }

        [Test]
        public void OnShoppingListIdChanged_NewShoppingList_LoadsShopsCategoriesAndUnits()
        {
            var shop = new ShopModel(Guid.NewGuid(), "Lidl");
            var category = new ProductCategoryModel(Guid.NewGuid(), "Dairy");
            var unit = new UnitModel(Guid.NewGuid(), "Liter", UnitType.Volume);
            SetupLoadDependencies([shop], [category], [unit]);

            _viewModel.ShoppingListId = Guid.Empty.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsNew, Is.True);
                Assert.That(_viewModel.Shops, Contains.Item(shop));
                Assert.That(_viewModel.ProductCategories, Has.Count.EqualTo(2));
                Assert.That(_viewModel.ProductCategories[0].Id, Is.EqualTo(Guid.Empty));
                Assert.That(_viewModel.ProductCategories[1], Is.EqualTo(category));
                Assert.That(_viewModel.ShoppingListProducts, Is.Empty);
                Assert.That(_viewModel.SelectedShop, Is.Null);
                Assert.That(_viewModel.IsBusy, Is.False);
            }

            _shoppingListServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _shopServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public void OnShoppingListIdChanged_ExistingShoppingList_LoadsModelAndSelectsMatchingShop()
        {
            var shop = new ShopModel(Guid.NewGuid(), "Lidl");
            SetupLoadDependencies([shop]);

            var id = Guid.NewGuid();
            var existing = new ShoppingListEditModel(id, "Week 1 list", shop.Id) { Products = [] };
            _shoppingListServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);
            // Selecting the matching shop triggers a lookup of its category display order.
            _shopServiceMock
                .Setup(s => s.GetEditAsync(shop.Id, CancellationToken.None))
                .ReturnsAsync((ShopEditModel?)null);

            _viewModel.ShoppingListId = id.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsNew, Is.False);
                Assert.That(_viewModel.Model.Name, Is.EqualTo("Week 1 list"));
                Assert.That(_viewModel.SelectedShop, Is.EqualTo(shop));
                Assert.That(_viewModel.Model.ShopId, Is.EqualTo(shop.Id));
                Assert.That(_viewModel.ShoppingListProducts, Is.Empty);
            }
        }

        [Test]
        public void OnSelectedShopChanged_LoadsDisplaySequenceAndResequencesProductsByCategory()
        {
            var categoryA = new ProductCategoryModel(Guid.NewGuid(), "Dairy");
            var categoryB = new ProductCategoryModel(Guid.NewGuid(), "Bakery");
            var shop = new ShopModel(Guid.NewGuid(), "Lidl");
            var shopDetails = new ShopEditModel
            {
                DisplaySequence =
                [
                    new ShopDisplaySequenceEditModel(shop.Id, 2, categoryA),
                    new ShopDisplaySequenceEditModel(shop.Id, 1, categoryB)
                ]
            };
            _shopServiceMock
                .Setup(s => s.GetEditAsync(shop.Id, CancellationToken.None))
                .ReturnsAsync(shopDetails);

            var productA = new ProductModel(Guid.NewGuid(), "Milk") { ProductCategory = categoryA };
            var productB = new ProductModel(Guid.NewGuid(), "Bread") { ProductCategory = categoryB };
            var itemA = new ShoppingListProductEditModel { Product = productA, Quantity = 1, DisplaySequence = 1, Collected = false };
            var itemB = new ShoppingListProductEditModel { Product = productB, Quantity = 1, DisplaySequence = 1, Collected = false };
            _viewModel.ShoppingListProducts = new ObservableCollection<ShoppingListProductEditModel> { itemA, itemB };

            _viewModel.SelectedShop = shop;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Model.ShopId, Is.EqualTo(shop.Id));
                Assert.That(itemA.DisplaySequence, Is.EqualTo(2));
                Assert.That(itemB.DisplaySequence, Is.EqualTo(1));
                Assert.That(_viewModel.ShoppingListProducts.Select(p => p.Product), Is.EqualTo(new[] { productB, productA }));
            }
        }

        [Test]
        public void ResequenceProducts_OrdersByCollectedThenDisplaySequenceThenName()
        {
            var shop = new ShopModel(Guid.NewGuid(), "Lidl");
            // Returning null skips category-based resequencing so only the ordering itself is exercised.
            _shopServiceMock
                .Setup(s => s.GetEditAsync(shop.Id, CancellationToken.None))
                .ReturnsAsync((ShopEditModel?)null);

            var collected = new ShoppingListProductEditModel { Product = new ProductModel(Guid.NewGuid(), "Zucchini"), Collected = true, DisplaySequence = 1 };
            var banana = new ShoppingListProductEditModel { Product = new ProductModel(Guid.NewGuid(), "Banana"), Collected = false, DisplaySequence = 2 };
            var apple = new ShoppingListProductEditModel { Product = new ProductModel(Guid.NewGuid(), "Apple"), Collected = false, DisplaySequence = 1 };
            var carrot = new ShoppingListProductEditModel { Product = new ProductModel(Guid.NewGuid(), "Carrot"), Collected = false, DisplaySequence = 1 };
            _viewModel.ShoppingListProducts = new ObservableCollection<ShoppingListProductEditModel> { collected, banana, apple, carrot };

            _viewModel.SelectedShop = shop;

            Assert.That(
                _viewModel.ShoppingListProducts.Select(p => p.Product!.Name),
                Is.EqualTo(new[] { "Apple", "Carrot", "Banana", "Zucchini" }));
        }

        [Test]
        public void AddProduct_NoSelectedProduct_DoesNotAddProduct()
        {
            _viewModel.SelectedProduct = null;
            _viewModel.SelectedUnit = new UnitModel(Guid.NewGuid(), "Liter", UnitType.Volume);
            _viewModel.QuantityText = "2";

            _viewModel.AddProductCommand.Execute(null);

            Assert.That(_viewModel.ShoppingListProducts, Is.Empty);
        }

        [Test]
        public void AddProduct_InvalidQuantity_DoesNotAddProduct()
        {
            _viewModel.SelectedProduct = new ProductModel(Guid.NewGuid(), "Milk");
            _viewModel.SelectedUnit = new UnitModel(Guid.NewGuid(), "Liter", UnitType.Volume);
            _viewModel.QuantityText = "not-a-number";

            _viewModel.AddProductCommand.Execute(null);

            Assert.That(_viewModel.ShoppingListProducts, Is.Empty);
        }

        [Test]
        public void AddProduct_NewProduct_AddsToShoppingListProducts()
        {
            var product = new ProductModel(Guid.NewGuid(), "Milk");
            var unit = new UnitModel(Guid.NewGuid(), "Liter", UnitType.Volume);
            _viewModel.SelectedProduct = product;
            _viewModel.SelectedUnit = unit;
            _viewModel.QuantityText = "2";

            _viewModel.AddProductCommand.Execute(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ShoppingListProducts, Has.Count.EqualTo(1));
                Assert.That(_viewModel.ShoppingListProducts[0].Product, Is.EqualTo(product));
                Assert.That(_viewModel.ShoppingListProducts[0].Quantity, Is.EqualTo(2));
                Assert.That(_viewModel.ShoppingListProducts[0].UnitId, Is.EqualTo(unit.Id));
                Assert.That(_viewModel.ShoppingListProducts[0].Collected, Is.False);
                Assert.That(_viewModel.QuantityText, Is.Empty);
                Assert.That(_viewModel.SelectedProduct, Is.Null);
            }
        }

        [Test]
        public void AddProduct_ExistingProductSameId_MergesQuantityInsteadOfDuplicate()
        {
            var product = new ProductModel(Guid.NewGuid(), "Milk");
            var existing = new ShoppingListProductEditModel { Product = product, Quantity = 3, DisplaySequence = 1 };
            _viewModel.ShoppingListProducts.Add(existing);

            _viewModel.SelectedProduct = new ProductModel(product.Id, "Milk");
            _viewModel.SelectedUnit = new UnitModel(Guid.NewGuid(), "Liter", UnitType.Volume);
            _viewModel.QuantityText = "2";

            _viewModel.AddProductCommand.Execute(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ShoppingListProducts, Has.Count.EqualTo(1));
                Assert.That(existing.Quantity, Is.EqualTo(5));
            }
        }

        [Test]
        public async Task AddFromMealPlanAsync_MergesNewAndExistingProductsByProductId()
        {
            var existingProduct = new ProductModel(Guid.NewGuid(), "Milk");
            var existingItem = new ShoppingListProductEditModel { Product = existingProduct, Quantity = 2, DisplaySequence = 1 };
            _viewModel.ShoppingListProducts.Add(existingItem);
            _viewModel.Model.ShopId = Guid.NewGuid();

            var plan = new MealPlanModel(Guid.NewGuid(), "Week 1");
            var newProduct = new ProductModel(Guid.NewGuid(), "Bread");
            IList<ShoppingListProductEditModel> incoming =
            [
                new ShoppingListProductEditModel { Product = existingProduct, Quantity = 3 },
                new ShoppingListProductEditModel { Product = newProduct, Quantity = 1 }
            ];
            _mealPlanServiceMock
                .Setup(s => s.GetShoppingListProductsAsync(plan.Id, _viewModel.Model.ShopId, CancellationToken.None))
                .ReturnsAsync(incoming);

            await _viewModel.AddFromMealPlanAsync(plan);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ShoppingListProducts, Has.Count.EqualTo(2));
                Assert.That(existingItem.Quantity, Is.EqualTo(5));
                var addedItem = _viewModel.ShoppingListProducts.Single(p => p.Product == newProduct);
                Assert.That(addedItem.Quantity, Is.EqualTo(1));
                Assert.That(addedItem.ShoppingListId, Is.EqualTo(_viewModel.Model.Id));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task AddFromRecipeAsync_MergesNewAndExistingProductsByProductId()
        {
            var existingProduct = new ProductModel(Guid.NewGuid(), "Milk");
            var existingItem = new ShoppingListProductEditModel { Product = existingProduct, Quantity = 1, DisplaySequence = 1 };
            _viewModel.ShoppingListProducts.Add(existingItem);
            _viewModel.Model.ShopId = Guid.NewGuid();

            var recipe = new RecipeModel(Guid.NewGuid(), "Pancakes");
            var newProduct = new ProductModel(Guid.NewGuid(), "Eggs");
            IList<ShoppingListProductEditModel> incoming =
            [
                new ShoppingListProductEditModel { Product = existingProduct, Quantity = 2 },
                new ShoppingListProductEditModel { Product = newProduct, Quantity = 4 }
            ];
            _recipeServiceMock
                .Setup(s => s.GetShoppingListProductsAsync(recipe.Id, _viewModel.Model.ShopId, CancellationToken.None))
                .ReturnsAsync(incoming);

            await _viewModel.AddFromRecipeAsync(recipe);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ShoppingListProducts, Has.Count.EqualTo(2));
                Assert.That(existingItem.Quantity, Is.EqualTo(3));
                var addedItem = _viewModel.ShoppingListProducts.Single(p => p.Product == newProduct);
                Assert.That(addedItem.Quantity, Is.EqualTo(4));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task LoadMealPlansForSelectionAsync_ShopNotSelected_ReturnsNullAndSetsError()
        {
            var result = await _viewModel.LoadMealPlansForSelectionAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Null);
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(MealPlannerSharedMessages.SelectShopFirst));
            }
            _mealPlanServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task LoadMealPlansForSelectionAsync_NoResults_ReturnsNullAndSetsError()
        {
            _viewModel.Model.ShopId = Guid.NewGuid();
            _mealPlanServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<MealPlanModel>([], Metadata.Create(1, 200, 0)));

            var result = await _viewModel.LoadMealPlansForSelectionAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Null);
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(MealPlannerSharedMessages.NoMealPlansFound));
            }
        }

        [Test]
        public async Task LoadMealPlansForSelectionAsync_Success_ReturnsItems()
        {
            _viewModel.Model.ShopId = Guid.NewGuid();
            var plan = new MealPlanModel(Guid.NewGuid(), "Week 1");
            _mealPlanServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<MealPlanModel>([plan], Metadata.Create(1, 200, 1)));

            var result = await _viewModel.LoadMealPlansForSelectionAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Contains.Item(plan));
                Assert.That(_viewModel.ErrorMessage, Is.Null);
            }
        }

        [Test]
        public async Task LoadMealPlansForSelectionAsync_ServiceThrows_ReturnsNullAndSetsError()
        {
            _viewModel.Model.ShopId = Guid.NewGuid();
            _mealPlanServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>(), CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var result = await _viewModel.LoadMealPlansForSelectionAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Null);
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("boom"));
            }
        }

        [Test]
        public async Task LoadRecipesForSelectionAsync_ShopNotSelected_ReturnsNullAndSetsError()
        {
            var result = await _viewModel.LoadRecipesForSelectionAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Null);
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(MealPlannerSharedMessages.SelectShopFirst));
            }
            _recipeServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task LoadRecipesForSelectionAsync_NoResults_ReturnsNullAndSetsError()
        {
            _viewModel.Model.ShopId = Guid.NewGuid();
            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeModel>([], Metadata.Create(1, 500, 0)));

            var result = await _viewModel.LoadRecipesForSelectionAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Null);
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(MealPlannerSharedMessages.NoRecipesFound));
            }
        }

        [Test]
        public async Task LoadRecipesForSelectionAsync_Success_ReturnsItems()
        {
            _viewModel.Model.ShopId = Guid.NewGuid();
            var recipe = new RecipeModel(Guid.NewGuid(), "Pancakes");
            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeModel>([recipe], Metadata.Create(1, 500, 1)));

            var result = await _viewModel.LoadRecipesForSelectionAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Contains.Item(recipe));
                Assert.That(_viewModel.ErrorMessage, Is.Null);
            }
        }

        [Test]
        public async Task ToggleCollectedAsync_NewShoppingList_TogglesCollectedMovesToEndAndDoesNotCallService()
        {
            _viewModel.IsNew = true;
            var item1 = new ShoppingListProductEditModel { Product = new ProductModel(Guid.NewGuid(), "Apple"), Collected = false, DisplaySequence = 1 };
            var item2 = new ShoppingListProductEditModel { Product = new ProductModel(Guid.NewGuid(), "Banana"), Collected = false, DisplaySequence = 2 };
            _viewModel.ShoppingListProducts = new ObservableCollection<ShoppingListProductEditModel> { item1, item2 };

            await _viewModel.ToggleCollectedCommand.ExecuteAsync(item1);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(item1.Collected, Is.True);
                Assert.That(_viewModel.ShoppingListProducts.Select(p => p.Product!.Name), Is.EqualTo(new[] { "Banana", "Apple" }));
                Assert.That(item2.DisplaySequence, Is.EqualTo(2));
                Assert.That(item1.DisplaySequence, Is.EqualTo(1));
            }
            _shoppingListServiceMock.Verify(
                s => s.UpdateProductCollectedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task ToggleCollectedAsync_MiddleOfLargerList_MovesOnlyToggledItem_LeavesOthersInPlace()
        {
            _viewModel.IsNew = true;
            var itemA = new ShoppingListProductEditModel { Product = new ProductModel(Guid.NewGuid(), "Apples"), Collected = false, DisplaySequence = 1 };
            var itemB = new ShoppingListProductEditModel { Product = new ProductModel(Guid.NewGuid(), "Bread"), Collected = false, DisplaySequence = 2 };
            var itemC = new ShoppingListProductEditModel { Product = new ProductModel(Guid.NewGuid(), "Cheese"), Collected = false, DisplaySequence = 3 };
            var itemD = new ShoppingListProductEditModel { Product = new ProductModel(Guid.NewGuid(), "Dates"), Collected = true, DisplaySequence = 1 };
            _viewModel.ShoppingListProducts = new ObservableCollection<ShoppingListProductEditModel> { itemA, itemB, itemC, itemD };

            await _viewModel.ToggleCollectedCommand.ExecuteAsync(itemC);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(itemC.Collected, Is.True);
                Assert.That(
                    _viewModel.ShoppingListProducts.Select(p => p.Product!.Name),
                    Is.EqualTo(new[] { "Apples", "Bread", "Dates", "Cheese" }));
            }
        }

        [Test]
        public async Task ToggleCollectedAsync_ItemAlreadyInCorrectPosition_DoesNotReorder()
        {
            _viewModel.IsNew = true;
            var itemA = new ShoppingListProductEditModel { Product = new ProductModel(Guid.NewGuid(), "Apple"), Collected = false, DisplaySequence = 1 };
            var itemB = new ShoppingListProductEditModel { Product = new ProductModel(Guid.NewGuid(), "Banana"), Collected = true, DisplaySequence = 1 };
            _viewModel.ShoppingListProducts = new ObservableCollection<ShoppingListProductEditModel> { itemA, itemB };

            // Uncollecting Banana still sorts it after Apple (same sequence, name tiebreak), so it stays at index 1.
            await _viewModel.ToggleCollectedCommand.ExecuteAsync(itemB);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(itemB.Collected, Is.False);
                Assert.That(
                    _viewModel.ShoppingListProducts.Select(p => p.Product!.Name),
                    Is.EqualTo(new[] { "Apple", "Banana" }));
            }
        }

        [Test]
        public async Task ToggleCollectedAsync_ExistingShoppingList_TogglesCollectedAndCallsUpdateProductCollectedAsync()
        {
            _viewModel.IsNew = false;
            var item1 = new ShoppingListProductEditModel { Product = new ProductModel(Guid.NewGuid(), "Apple"), Collected = false, DisplaySequence = 1 };
            _viewModel.ShoppingListProducts = new ObservableCollection<ShoppingListProductEditModel> { item1 };

            _shoppingListServiceMock
                .Setup(s => s.UpdateProductCollectedAsync(_viewModel.Model.Id, item1.Product!.Id, true, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.ToggleCollectedCommand.ExecuteAsync(item1);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(item1.Collected, Is.True);
                Assert.That(_viewModel.ErrorMessage, Is.Null);
            }
            _shoppingListServiceMock.Verify(
                s => s.UpdateProductCollectedAsync(_viewModel.Model.Id, item1.Product!.Id, true, CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task ToggleCollectedAsync_ExistingShoppingListUpdateFails_SetsErrorMessage()
        {
            _viewModel.IsNew = false;
            var item1 = new ShoppingListProductEditModel { Product = new ProductModel(Guid.NewGuid(), "Apple"), Collected = false, DisplaySequence = 1 };
            _viewModel.ShoppingListProducts = new ObservableCollection<ShoppingListProductEditModel> { item1 };

            _shoppingListServiceMock
                .Setup(s => s.UpdateProductCollectedAsync(_viewModel.Model.Id, item1.Product!.Id, true, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("cannot update"));

            await _viewModel.ToggleCollectedCommand.ExecuteAsync(item1);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo("cannot update"));
        }

        [Test]
        public async Task SaveAsync_NameMissing_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Name = string.Empty;
            _viewModel.Model.ShopId = Guid.NewGuid();
            _viewModel.ShoppingListProducts.Add(new ShoppingListProductEditModel { Product = new ProductModel(Guid.NewGuid(), "Milk"), DisplaySequence = 1 });

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(MealPlannerSharedMessages.ShoppingListNameRequired));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _shoppingListServiceMock.Verify(
                s => s.AddAsync(It.IsAny<ShoppingListEditModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _shoppingListServiceMock.Verify(
                s => s.UpdateAsync(It.IsAny<ShoppingListEditModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task SaveAsync_ShopNotSelected_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Name = "Week 1 list";
            _viewModel.Model.ShopId = Guid.Empty;
            _viewModel.ShoppingListProducts.Add(new ShoppingListProductEditModel { Product = new ProductModel(Guid.NewGuid(), "Milk"), DisplaySequence = 1 });

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(MealPlannerSharedMessages.SelectShopFirst));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _shoppingListServiceMock.Verify(
                s => s.AddAsync(It.IsAny<ShoppingListEditModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task SaveAsync_NoProducts_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Name = "Week 1 list";
            _viewModel.Model.ShopId = Guid.NewGuid();

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(MealPlannerSharedMessages.ShoppingListRequiresProducts));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _shoppingListServiceMock.Verify(
                s => s.AddAsync(It.IsAny<ShoppingListEditModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task SaveAsync_NewShoppingListValid_CallsAddAsync()
        {
            _viewModel.IsNew = true;
            _viewModel.Model.Name = "Week 1 list";
            _viewModel.Model.ShopId = Guid.NewGuid();
            _viewModel.ShoppingListProducts.Add(new ShoppingListProductEditModel { Product = new ProductModel(Guid.NewGuid(), "Milk"), DisplaySequence = 1 });

            _shoppingListServiceMock
                .Setup(s => s.AddAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            // Shell.Current.GoToAsync runs after a successful save inside the try/catch, so the
            // resulting NullReferenceException in this test host is swallowed into ErrorMessage.
            // Only the service call is verified here.
            await _viewModel.SaveCommand.ExecuteAsync(null);

            _shoppingListServiceMock.Verify(s => s.AddAsync(_viewModel.Model, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task SaveAsync_ExistingShoppingListValid_CallsUpdateAsync()
        {
            _viewModel.IsNew = false;
            _viewModel.Model.Name = "Week 1 list";
            _viewModel.Model.ShopId = Guid.NewGuid();
            _viewModel.ShoppingListProducts.Add(new ShoppingListProductEditModel { Product = new ProductModel(Guid.NewGuid(), "Milk"), DisplaySequence = 1 });

            _shoppingListServiceMock
                .Setup(s => s.UpdateAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.SaveCommand.ExecuteAsync(null);

            _shoppingListServiceMock.Verify(s => s.UpdateAsync(_viewModel.Model, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_WhenIsNew_ReturnsWithoutCallingService()
        {
            // DeleteAsync confirms via Shell.Current.DisplayAlertAsync before any try/catch, so
            // calling it past the IsNew guard would throw in this test host. Only the guard-clause
            // return path is exercised here.
            _viewModel.IsNew = true;

            await _viewModel.DeleteCommand.ExecuteAsync(null);

            _shoppingListServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
