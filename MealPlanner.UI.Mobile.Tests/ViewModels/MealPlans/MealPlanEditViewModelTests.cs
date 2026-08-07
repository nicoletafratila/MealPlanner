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
    public class MealPlanEditViewModelTests
    {
        private Mock<IMealPlanService> _mealPlanServiceMock = null!;
        private Mock<IRecipeService> _recipeServiceMock = null!;
        private Mock<IRecipeCategoryService> _recipeCategoryServiceMock = null!;
        private Mock<IShopService> _shopServiceMock = null!;
        private Mock<IShoppingListService> _shoppingListServiceMock = null!;
        private Mock<IRecipeCategoryService> _lookupRecipeCategoryServiceMock = null!;
        private Mock<IUnitService> _lookupUnitServiceMock = null!;
        private Mock<IProductService> _lookupProductServiceMock = null!;
        private Mock<IProductCategoryService> _lookupProductCategoryServiceMock = null!;
        private MealPlanEditViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _mealPlanServiceMock = new Mock<IMealPlanService>(MockBehavior.Strict);
            _recipeServiceMock = new Mock<IRecipeService>(MockBehavior.Strict);
            _recipeCategoryServiceMock = new Mock<IRecipeCategoryService>(MockBehavior.Strict);
            _shopServiceMock = new Mock<IShopService>(MockBehavior.Strict);
            _shoppingListServiceMock = new Mock<IShoppingListService>(MockBehavior.Strict);
            _lookupRecipeCategoryServiceMock = new Mock<IRecipeCategoryService>(MockBehavior.Strict);
            _lookupUnitServiceMock = new Mock<IUnitService>(MockBehavior.Strict);
            _lookupProductServiceMock = new Mock<IProductService>(MockBehavior.Strict);
            _lookupProductCategoryServiceMock = new Mock<IProductCategoryService>(MockBehavior.Strict);

            var lookupDataService = new ReferenceDataCacheService(
                _lookupRecipeCategoryServiceMock.Object,
                _lookupUnitServiceMock.Object,
                _lookupProductServiceMock.Object,
                _lookupProductCategoryServiceMock.Object,
                _shopServiceMock.Object,
                _recipeServiceMock.Object);

            _viewModel = new MealPlanEditViewModel(
                _mealPlanServiceMock.Object,
                _recipeCategoryServiceMock.Object,
                _shoppingListServiceMock.Object,
                lookupDataService);
        }

        private void SetupLoadDependencies(
            IReadOnlyList<RecipeCategoryModel>? categories = null,
            IReadOnlyList<RecipeModel>? recipes = null,
            IReadOnlyList<ShopModel>? shops = null)
        {
            categories ??= [];
            recipes ??= [];
            shops ??= [];

            _recipeCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>(categories, Metadata.Create(1, 200, categories.Count)));
            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeModel>(recipes, Metadata.Create(1, 500, recipes.Count)));
            _shopServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ShopModel>(shops, Metadata.Create(1, 200, shops.Count)));
            _mealPlanServiceMock
                .Setup(s => s.GetMenuName(It.IsAny<string>()))
                .Returns("Meniu 2026/1");
            _lookupRecipeCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([], Metadata.Create(1, 100, 0)));
            _lookupUnitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<UnitModel>([], Metadata.Create(1, 100, 0)));
            _lookupProductCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], Metadata.Create(1, 200, 0)));
        }

        [Test]
        public void OnMealPlanIdChanged_NewMealPlan_LoadsCategoriesRecipesAndShops()
        {
            var category = new RecipeCategoryModel(Guid.NewGuid(), "Dessert");
            var recipe = new RecipeModel(Guid.NewGuid(), "Pancakes");
            var shop = new ShopModel(Guid.NewGuid(), "Lidl");
            SetupLoadDependencies([category], [recipe], [shop]);

            _viewModel.MealPlanId = Guid.Empty.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsNew, Is.True);
                Assert.That(_viewModel.Categories, Has.Count.EqualTo(2));
                Assert.That(_viewModel.Categories[0].Id, Is.EqualTo(Guid.Empty));
                Assert.That(_viewModel.Categories[1], Is.EqualTo(category));
                Assert.That(_viewModel.AllRecipes, Contains.Item(recipe));
                Assert.That(_viewModel.FilteredRecipes, Contains.Item(recipe));
                Assert.That(_viewModel.Shops, Contains.Item(shop));
                Assert.That(_viewModel.PlanRecipes, Is.Empty);
                Assert.That(_viewModel.IsBusy, Is.False);
            }

            _mealPlanServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public void OnMealPlanIdChanged_NewMealPlan_SuggestsMenuName()
        {
            SetupLoadDependencies();

            _viewModel.MealPlanId = Guid.Empty.ToString();

            Assert.That(_viewModel.Model.Name, Is.EqualTo("Meniu 2026/1"));
        }

        [Test]
        public void OnMealPlanIdChanged_ExistingMealPlan_LoadsPlanRecipesFromService()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pancakes");
            SetupLoadDependencies(recipes: [recipe]);

            var id = Guid.NewGuid();
            var existing = new MealPlanEditModel(id, "Week 1") { Recipes = [recipe] };
            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);

            _viewModel.MealPlanId = id.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsNew, Is.False);
                Assert.That(_viewModel.Model.Name, Is.EqualTo("Week 1"));
                Assert.That(_viewModel.PlanRecipes, Has.Count.EqualTo(1));
                Assert.That(_viewModel.PlanRecipes, Contains.Item(recipe));
            }
        }

        [Test]
        public void LoadAsync_NewMealPlanWithPreselectedRecipeId_AddsRecipeToPlan()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pancakes");
            SetupLoadDependencies(recipes: [recipe]);

            _viewModel.PreselectedRecipeId = recipe.Id.ToString();
            _viewModel.MealPlanId = Guid.Empty.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.PlanRecipes, Has.Count.EqualTo(1));
                Assert.That(_viewModel.PlanRecipes, Contains.Item(recipe));
            }
        }

        [Test]
        public void LoadAsync_ExistingMealPlanWithPreselectedRecipeAlreadyInPlan_DoesNotAddDuplicate()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pancakes");
            SetupLoadDependencies(recipes: [recipe]);

            var id = Guid.NewGuid();
            var existing = new MealPlanEditModel(id, "Week 1") { Recipes = [recipe] };
            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);

            _viewModel.PreselectedRecipeId = recipe.Id.ToString();
            _viewModel.MealPlanId = id.ToString();

            Assert.That(_viewModel.PlanRecipes, Has.Count.EqualTo(1));
        }

        [Test]
        public void OnSelectedCategoryChanged_WithSpecificCategory_FiltersRecipesByCategory()
        {
            var categoryId = Guid.NewGuid();
            var category = new RecipeCategoryModel(categoryId, "Dessert");
            var matchingRecipe = new RecipeModel(Guid.NewGuid(), "Cake") { RecipeCategoryId = categoryId.ToString() };
            var otherRecipe = new RecipeModel(Guid.NewGuid(), "Soup") { RecipeCategoryId = Guid.NewGuid().ToString() };
            SetupLoadDependencies(categories: [category], recipes: [matchingRecipe, otherRecipe]);

            _viewModel.MealPlanId = Guid.Empty.ToString();
            _viewModel.SelectedRecipe = matchingRecipe;

            _viewModel.SelectedCategory = category;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.FilteredRecipes, Has.Count.EqualTo(1));
                Assert.That(_viewModel.FilteredRecipes, Contains.Item(matchingRecipe));
                Assert.That(_viewModel.SelectedRecipe, Is.Null);
            }
        }

        [Test]
        public void OnSelectedCategoryChanged_WithAllCategoriesEntry_ShowsAllRecipes()
        {
            var recipe1 = new RecipeModel(Guid.NewGuid(), "Cake");
            var recipe2 = new RecipeModel(Guid.NewGuid(), "Soup");
            SetupLoadDependencies(recipes: [recipe1, recipe2]);

            _viewModel.MealPlanId = Guid.Empty.ToString();

            _viewModel.SelectedCategory = _viewModel.Categories[0];

            Assert.That(_viewModel.FilteredRecipes, Has.Count.EqualTo(2));
        }

        [Test]
        public void OnSelectedCategoryChanged_WithNull_ShowsAllRecipes()
        {
            var recipe1 = new RecipeModel(Guid.NewGuid(), "Cake");
            SetupLoadDependencies(recipes: [recipe1]);

            _viewModel.MealPlanId = Guid.Empty.ToString();

            _viewModel.SelectedCategory = null;

            Assert.That(_viewModel.FilteredRecipes, Has.Count.EqualTo(1));
        }

        [Test]
        public void AddRecipe_NoSelectedRecipe_DoesNotAddToPlan()
        {
            _viewModel.SelectedRecipe = null;

            _viewModel.AddRecipeCommand.Execute(null);

            Assert.That(_viewModel.PlanRecipes, Is.Empty);
        }

        [Test]
        public void AddRecipe_NewRecipe_AddsToPlanAndClearsSelection()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pancakes");
            _viewModel.SelectedRecipe = recipe;

            _viewModel.AddRecipeCommand.Execute(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.PlanRecipes, Contains.Item(recipe));
                Assert.That(_viewModel.SelectedRecipe, Is.Null);
            }
        }

        [Test]
        public void AddRecipe_AlreadyInPlan_DoesNotAddDuplicate()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pancakes");
            _viewModel.PlanRecipes.Add(recipe);
            _viewModel.SelectedRecipe = recipe;

            _viewModel.AddRecipeCommand.Execute(null);

            Assert.That(_viewModel.PlanRecipes, Has.Count.EqualTo(1));
        }

        [Test]
        public void RemoveRecipe_RemovesRecipeFromPlan()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pancakes");
            _viewModel.PlanRecipes.Add(recipe);

            _viewModel.RemoveRecipeCommand.Execute(recipe);

            Assert.That(_viewModel.PlanRecipes, Does.Not.Contain(recipe));
        }

        [Test]
        public async Task SaveAsync_NameMissing_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Name = string.Empty;
            _viewModel.PlanRecipes.Add(new RecipeModel(Guid.NewGuid(), "Pancakes"));

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(MealPlannerSharedMessages.MealPlanNameRequired));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _mealPlanServiceMock.Verify(
                s => s.AddAsync(It.IsAny<MealPlanEditModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _mealPlanServiceMock.Verify(
                s => s.UpdateAsync(It.IsAny<MealPlanEditModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task SaveAsync_NoRecipes_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Name = "Week 1";

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(MealPlannerSharedMessages.MealPlanRequiresRecipes));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _mealPlanServiceMock.Verify(
                s => s.AddAsync(It.IsAny<MealPlanEditModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task SaveAsync_NewMealPlanValid_CallsAddAsync()
        {
            SetupLoadDependencies();
            _viewModel.MealPlanId = Guid.Empty.ToString();
            _viewModel.Model.Name = "Week 1";
            _viewModel.PlanRecipes.Add(new RecipeModel(Guid.NewGuid(), "Pancakes"));

            _mealPlanServiceMock
                .Setup(s => s.AddAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            // Shell.Current.GoToAsync runs after a successful save inside the try/catch, so the
            // resulting NullReferenceException in this test host is swallowed into ErrorMessage.
            // Only the service call is verified here.
            await _viewModel.SaveCommand.ExecuteAsync(null);

            _mealPlanServiceMock.Verify(s => s.AddAsync(_viewModel.Model, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task SaveAsync_ExistingMealPlanValid_CallsUpdateAsync()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pancakes");
            SetupLoadDependencies(recipes: [recipe]);
            var id = Guid.NewGuid();
            var existing = new MealPlanEditModel(id, "Week 1") { Recipes = [recipe] };
            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);
            _viewModel.MealPlanId = id.ToString();

            _viewModel.Model.Name = "Week 1 updated";

            _mealPlanServiceMock
                .Setup(s => s.UpdateAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.SaveCommand.ExecuteAsync(null);

            _mealPlanServiceMock.Verify(s => s.UpdateAsync(_viewModel.Model, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_WhenIsNew_ReturnsWithoutCallingService()
        {
            // DeleteAsync confirms via Shell.Current.DisplayAlertAsync before any try/catch, so
            // calling it past the IsNew guard would throw in this test host. Only the guard-clause
            // return path is exercised here.
            _viewModel.MealPlanId = Guid.Empty.ToString();
            Assert.That(_viewModel.IsNew, Is.True);

            await _viewModel.DeleteCommand.ExecuteAsync(null);

            _mealPlanServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task MakeShoppingListAsync_WhenIsNew_DoesNothing()
        {
            SetupLoadDependencies();
            _viewModel.MealPlanId = Guid.Empty.ToString();
            _viewModel.PlanRecipes.Add(new RecipeModel(Guid.NewGuid(), "Pancakes"));

            await _viewModel.MakeShoppingListCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.Null);
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _shoppingListServiceMock.Verify(
                s => s.MakeShoppingListAsync(It.IsAny<ShoppingListCreateModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task MakeShoppingListAsync_WhenPlanHasNoRecipes_DoesNothing()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pancakes");
            SetupLoadDependencies(recipes: [recipe]);
            var id = Guid.NewGuid();
            var existing = new MealPlanEditModel(id, "Week 1") { Recipes = [] };
            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);
            _viewModel.MealPlanId = id.ToString();

            await _viewModel.MakeShoppingListCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.Null);
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _shoppingListServiceMock.Verify(
                s => s.MakeShoppingListAsync(It.IsAny<ShoppingListCreateModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task MakeShoppingListAsync_WhenNoShopsAvailable_SetsError()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pancakes");
            SetupLoadDependencies(recipes: [recipe]);
            var id = Guid.NewGuid();
            var existing = new MealPlanEditModel(id, "Week 1") { Recipes = [recipe] };
            _mealPlanServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);
            _viewModel.MealPlanId = id.ToString();
            Assert.That(_viewModel.Shops, Is.Empty);

            await _viewModel.MakeShoppingListCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(MealPlannerSharedMessages.NoShopsAvailable));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _shoppingListServiceMock.Verify(
                s => s.MakeShoppingListAsync(It.IsAny<ShoppingListCreateModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
