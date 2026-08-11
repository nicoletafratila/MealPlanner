using Common.Models;
using Common.Pagination;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Mobile.Services;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.RecipeBook
{
    [TestFixture]
    public class RecipesOverviewViewModelTests
    {
        // Literal copies of the internal RecipesOverviewPage resx values used by the
        // production code. The resx-generated class is `internal` in MealPlanner.UI.Mobile
        // and this test project has no InternalsVisibleTo, so exact literals are asserted instead.
        private const string SaveFailedMessage = "Save failed. Please try again.";
        private const string MealPlanCreatedAndRecipeAdded = "Week's menu has been created and the recipe has been added successfully.";
        private const string RecipeAdded = "Recipe has been added successfully";

        private Mock<IRecipeService> _recipeServiceMock = null!;
        private Mock<IRecipeCategoryService> _categoryServiceMock = null!;
        private Mock<IMealPlanService> _mealPlanServiceMock = null!;
        private Mock<IUnitService> _lookupUnitServiceMock = null!;
        private Mock<IProductCategoryService> _lookupProductCategoryServiceMock = null!;
        private Mock<IShopService> _lookupShopServiceMock = null!;
        private Mock<IProductService> _lookupProductServiceMock = null!;
        private RecipesOverviewViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _recipeServiceMock = new Mock<IRecipeService>(MockBehavior.Strict);
            _categoryServiceMock = new Mock<IRecipeCategoryService>(MockBehavior.Strict);
            _mealPlanServiceMock = new Mock<IMealPlanService>(MockBehavior.Strict);
            _lookupUnitServiceMock = new Mock<IUnitService>(MockBehavior.Strict);
            _lookupProductCategoryServiceMock = new Mock<IProductCategoryService>(MockBehavior.Strict);
            _lookupShopServiceMock = new Mock<IShopService>(MockBehavior.Strict);
            _lookupProductServiceMock = new Mock<IProductService>(MockBehavior.Strict);

            var lookupDataService = new ReferenceDataCacheService(
                _categoryServiceMock.Object,
                _lookupUnitServiceMock.Object,
                _lookupProductServiceMock.Object,
                _lookupProductCategoryServiceMock.Object,
                _lookupShopServiceMock.Object,
                _recipeServiceMock.Object);

            _viewModel = new RecipesOverviewViewModel(_recipeServiceMock.Object, lookupDataService, _mealPlanServiceMock.Object);
        }

        private void SetupDummyLookups()
        {
            _lookupUnitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<UnitModel>([], Metadata.Create(1, 100, 0)));
            _lookupProductCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], Metadata.Create(1, 200, 0)));
            _lookupShopServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ShopModel>([], Metadata.Create(1, 200, 0)));
        }

        private static PagedList<RecipeModel> RecipesPage(IEnumerable<RecipeModel> items, int pageNumber, int pageSize, int totalCount) =>
            new(items, Metadata.Create(pageNumber, pageSize, totalCount));

        private static PagedList<RecipeCategoryModel> CategoriesPage(IEnumerable<RecipeCategoryModel> items) =>
            new(items, Metadata.Create(1, 100, items.Count()));

        [Test]
        public async Task LoadAsync_WhenBusy_DoesNotCallServices()
        {
            _viewModel.IsBusy = true;

            await _viewModel.LoadCommand.ExecuteAsync(null);

            _categoryServiceMock.Verify(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>(), It.IsAny<CancellationToken>()), Times.Never);
            _recipeServiceMock.Verify(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task LoadAsync_Success_LoadsCategoriesAndSearchesRecipes()
        {
            SetupDummyLookups();
            var categories = new List<RecipeCategoryModel> { new(Guid.NewGuid(), "Desert") };
            var recipes = new List<RecipeModel> { new(Guid.NewGuid(), "Pasta") };

            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<RecipeCategoryModel>>(p => p.PageSize == 100), CancellationToken.None))
                .ReturnsAsync(CategoriesPage(categories));

            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<RecipeModel>>(p => p.PageNumber == 1 && p.Filters == null), CancellationToken.None))
                .ReturnsAsync(RecipesPage(recipes, 1, 20, 30));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Categories, Has.Count.EqualTo(1));
                Assert.That(_viewModel.Recipes, Has.Count.EqualTo(1));
                Assert.That(_viewModel.HasNextPage, Is.True);
                Assert.That(_viewModel.IsBusy, Is.False);
                Assert.That(_viewModel.CurrentPage, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task LoadAsync_CategoryServiceThrows_SetsErrorMessage()
        {
            SetupDummyLookups();
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>(), CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));

            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), CancellationToken.None))
                .ReturnsAsync(RecipesPage([], 1, 20, 0));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("boom"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task SearchAsync_NoSearchTextNoCategory_PassesNullFilters()
        {
            QueryParameters<RecipeModel>? captured = null;
            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), CancellationToken.None))
                .Callback<QueryParameters<RecipeModel>, CancellationToken>((p, _) => captured = p)
                .ReturnsAsync(RecipesPage([], 1, 20, 0));

            await _viewModel.SearchCommand.ExecuteAsync(null);

            Assert.That(captured!.Filters, Is.Null);
        }

        [Test]
        public async Task SearchAsync_WithSearchTextOnly_AddsNameContainsFilter()
        {
            _viewModel.SearchText = "past";

            QueryParameters<RecipeModel>? captured = null;
            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), CancellationToken.None))
                .Callback<QueryParameters<RecipeModel>, CancellationToken>((p, _) => captured = p)
                .ReturnsAsync(RecipesPage([], 1, 20, 0));

            await _viewModel.SearchCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(captured!.Filters, Is.Not.Null);
                var filter = captured.Filters!.Single();
                Assert.That(filter.PropertyName, Is.EqualTo("Name"));
                Assert.That(filter.Value, Is.EqualTo("past"));
                Assert.That(filter.Operator, Is.EqualTo(FilterOperator.Contains));
                Assert.That(filter.StringComparison, Is.EqualTo(StringComparison.OrdinalIgnoreCase));
            }
        }

        [Test]
        public void SearchAsync_WithSearchTextAndCategory_AddsBothFilters()
        {
            var category = new RecipeCategoryModel(Guid.NewGuid(), "Desert");
            _viewModel.SearchText = "past";

            QueryParameters<RecipeModel>? captured = null;
            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), CancellationToken.None))
                .Callback<QueryParameters<RecipeModel>, CancellationToken>((p, _) => captured = p)
                .ReturnsAsync(RecipesPage([], 1, 20, 0));

            // Setting SelectedCategory triggers a search via OnSelectedCategoryChanged.
            _viewModel.SelectedCategory = category;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(captured, Is.Not.Null);
                Assert.That(captured!.Filters, Is.Not.Null);
                Assert.That(captured.Filters!.Count(), Is.EqualTo(2));
                var nameFilter = captured.Filters!.Single(f => f.PropertyName == "Name");
                Assert.That(nameFilter.Value, Is.EqualTo("past"));
                var categoryFilter = captured.Filters!.Single(f => f.PropertyName == "RecipeCategoryId");
                Assert.That(categoryFilter.Value, Is.EqualTo(category.Id.ToString()));
                Assert.That(categoryFilter.Operator, Is.EqualTo(FilterOperator.Equals));
            }
        }

        [Test]
        public void ClearCategory_ClearsSelectedCategoryAndTriggersSearchWithoutCategoryFilter()
        {
            var category = new RecipeCategoryModel(Guid.NewGuid(), "Desert");

            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), CancellationToken.None))
                .ReturnsAsync(RecipesPage([], 1, 20, 0));

            // Setting SelectedCategory triggers a search (consumed by the setup above).
            _viewModel.SelectedCategory = category;

            QueryParameters<RecipeModel>? captured = null;
            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), CancellationToken.None))
                .Callback<QueryParameters<RecipeModel>, CancellationToken>((p, _) => captured = p)
                .ReturnsAsync(RecipesPage([], 1, 20, 0));

            _viewModel.ClearCategoryCommand.Execute(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.SelectedCategory, Is.Null);
                Assert.That(captured!.Filters, Is.Null);
            }
        }

        [Test]
        public async Task NextPageAsync_WhenHasNextPage_AppendsItemsAndIncrementsPage()
        {
            var firstItem = new RecipeModel(Guid.NewGuid(), "Pasta");
            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), CancellationToken.None))
                .ReturnsAsync(RecipesPage([firstItem], 1, 20, 40));

            await _viewModel.SearchCommand.ExecuteAsync(null);

            var secondItem = new RecipeModel(Guid.NewGuid(), "Salad");
            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<RecipeModel>>(p => p.PageNumber == 2), CancellationToken.None))
                .ReturnsAsync(RecipesPage([secondItem], 2, 20, 40));

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Recipes, Has.Count.EqualTo(2));
                Assert.That(_viewModel.CurrentPage, Is.EqualTo(2));
                Assert.That(_viewModel.IsLoadingMore, Is.False);
                Assert.That(_viewModel.HasNextPage, Is.False);
            }
        }

        [Test]
        public async Task NextPageAsync_WhenNoNextPage_DoesNotCallService()
        {
            _viewModel.HasNextPage = false;

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            _recipeServiceMock.Verify(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task NextPageAsync_WhenIsBusy_DoesNotCallService()
        {
            _viewModel.HasNextPage = true;
            _viewModel.IsBusy = true;

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            _recipeServiceMock.Verify(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task NextPageAsync_WhenIsLoadingMore_DoesNotCallService()
        {
            _viewModel.HasNextPage = true;
            _viewModel.IsLoadingMore = true;

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            _recipeServiceMock.Verify(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task DeleteRecipeAsync_Success_RemovesRecipeFromCollection()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pasta");
            _viewModel.Recipes.Add(recipe);

            _recipeServiceMock
                .Setup(s => s.DeleteAsync(recipe.Id, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.DeleteRecipeCommand.ExecuteAsync(recipe);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Recipes, Does.Not.Contain(recipe));
                Assert.That(_viewModel.ErrorMessage, Is.Null);
            }
        }

        [Test]
        public async Task DeleteRecipeAsync_Failure_SetsErrorMessageAndKeepsRecipe()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pasta");
            _viewModel.Recipes.Add(recipe);

            _recipeServiceMock
                .Setup(s => s.DeleteAsync(recipe.Id, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("cannot delete"));

            await _viewModel.DeleteRecipeCommand.ExecuteAsync(recipe);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Recipes, Contains.Item(recipe));
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("cannot delete"));
            }
        }

        [Test]
        public async Task DeleteRecipeAsync_ServiceThrows_SetsErrorMessage()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pasta");
            _viewModel.Recipes.Add(recipe);

            _recipeServiceMock
                .Setup(s => s.DeleteAsync(recipe.Id, CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await _viewModel.DeleteRecipeCommand.ExecuteAsync(recipe);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Recipes, Contains.Item(recipe));
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("boom"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task DeleteRecipeAsync_WhenBusy_DoesNotCallService()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pasta");
            _viewModel.IsBusy = true;

            await _viewModel.DeleteRecipeCommand.ExecuteAsync(recipe);

            _recipeServiceMock.Verify(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task AddToMealPlanAsync_NoCurrentMealPlan_CreatesNewPlanAndSetsSuccess()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pasta");

            _mealPlanServiceMock.Setup(s => s.GetCurrentAsync(CancellationToken.None)).ReturnsAsync((MealPlanModel?)null);
            _mealPlanServiceMock.Setup(s => s.GetMenuName(It.IsAny<string>())).Returns("Menu");

            MealPlanEditModel? captured = null;
            _mealPlanServiceMock
                .Setup(s => s.AddAsync(It.IsAny<MealPlanEditModel>(), CancellationToken.None))
                .Callback<MealPlanEditModel, CancellationToken>((m, _) => captured = m)
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.AddToMealPlanCommand.ExecuteAsync(recipe);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.SuccessMessage, Is.EqualTo(MealPlanCreatedAndRecipeAdded));
                Assert.That(_viewModel.ErrorMessage, Is.Null);
                Assert.That(captured, Is.Not.Null);
                Assert.That(captured!.Name, Is.EqualTo("Menu"));
                Assert.That(captured.Recipes, Is.EquivalentTo(new[] { recipe }));
            }
        }

        [Test]
        public async Task AddToMealPlanAsync_NoCurrentMealPlan_CreateFailsWithMessage_SetsErrorFromResponse()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pasta");

            _mealPlanServiceMock.Setup(s => s.GetCurrentAsync(CancellationToken.None)).ReturnsAsync((MealPlanModel?)null);
            _mealPlanServiceMock.Setup(s => s.GetMenuName(It.IsAny<string>())).Returns("Menu");
            _mealPlanServiceMock
                .Setup(s => s.AddAsync(It.IsAny<MealPlanEditModel>(), CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("custom failure"));

            await _viewModel.AddToMealPlanCommand.ExecuteAsync(recipe);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo("custom failure"));
        }

        [Test]
        public async Task AddToMealPlanAsync_NoCurrentMealPlan_CreateFailsWithNullResponse_SetsFallbackErrorMessage()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pasta");

            _mealPlanServiceMock.Setup(s => s.GetCurrentAsync(CancellationToken.None)).ReturnsAsync((MealPlanModel?)null);
            _mealPlanServiceMock.Setup(s => s.GetMenuName(It.IsAny<string>())).Returns("Menu");
            _mealPlanServiceMock
                .Setup(s => s.AddAsync(It.IsAny<MealPlanEditModel>(), CancellationToken.None))
                .ReturnsAsync((CommandResponse?)null);

            await _viewModel.AddToMealPlanCommand.ExecuteAsync(recipe);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(SaveFailedMessage));
        }

        [Test]
        public async Task AddToMealPlanAsync_CurrentMealPlanExists_AddsRecipeAndUpdatesPlan()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pasta");
            var currentPlan = new MealPlanModel(Guid.NewGuid(), "This week");
            var existingRecipe = new RecipeModel(Guid.NewGuid(), "Salad");
            var editModel = new MealPlanEditModel(currentPlan.Id, "This week") { Recipes = [existingRecipe] };

            _mealPlanServiceMock.Setup(s => s.GetCurrentAsync(CancellationToken.None)).ReturnsAsync(currentPlan);
            _mealPlanServiceMock.Setup(s => s.GetEditAsync(currentPlan.Id, CancellationToken.None)).ReturnsAsync(editModel);

            MealPlanEditModel? captured = null;
            _mealPlanServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<MealPlanEditModel>(), CancellationToken.None))
                .Callback<MealPlanEditModel, CancellationToken>((m, _) => captured = m)
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.AddToMealPlanCommand.ExecuteAsync(recipe);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.SuccessMessage, Is.EqualTo(RecipeAdded));
                Assert.That(_viewModel.ErrorMessage, Is.Null);
                Assert.That(captured, Is.SameAs(editModel));
                Assert.That(captured!.Recipes, Has.Count.EqualTo(2));
                Assert.That(captured.Recipes, Contains.Item(recipe));
                Assert.That(existingRecipe.Index, Is.EqualTo(1));
                Assert.That(recipe.Index, Is.EqualTo(2));
            }
        }

        [Test]
        public async Task AddToMealPlanAsync_CurrentMealPlanExists_GetEditReturnsNull_SetsErrorMessage()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pasta");
            var currentPlan = new MealPlanModel(Guid.NewGuid(), "This week");

            _mealPlanServiceMock.Setup(s => s.GetCurrentAsync(CancellationToken.None)).ReturnsAsync(currentPlan);
            _mealPlanServiceMock.Setup(s => s.GetEditAsync(currentPlan.Id, CancellationToken.None)).ReturnsAsync((MealPlanEditModel?)null);

            await _viewModel.AddToMealPlanCommand.ExecuteAsync(recipe);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(SaveFailedMessage));
        }

        [Test]
        public async Task AddToMealPlanAsync_CurrentMealPlanExists_UpdateFailsWithMessage_SetsErrorFromResponse()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pasta");
            var currentPlan = new MealPlanModel(Guid.NewGuid(), "This week");
            var editModel = new MealPlanEditModel(currentPlan.Id, "This week") { Recipes = [] };

            _mealPlanServiceMock.Setup(s => s.GetCurrentAsync(CancellationToken.None)).ReturnsAsync(currentPlan);
            _mealPlanServiceMock.Setup(s => s.GetEditAsync(currentPlan.Id, CancellationToken.None)).ReturnsAsync(editModel);
            _mealPlanServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<MealPlanEditModel>(), CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("update failure"));

            await _viewModel.AddToMealPlanCommand.ExecuteAsync(recipe);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo("update failure"));
        }

        [Test]
        public async Task AddToMealPlanAsync_CurrentMealPlanExists_UpdateFailsWithNullResponse_SetsFallbackErrorMessage()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pasta");
            var currentPlan = new MealPlanModel(Guid.NewGuid(), "This week");
            var editModel = new MealPlanEditModel(currentPlan.Id, "This week") { Recipes = [] };

            _mealPlanServiceMock.Setup(s => s.GetCurrentAsync(CancellationToken.None)).ReturnsAsync(currentPlan);
            _mealPlanServiceMock.Setup(s => s.GetEditAsync(currentPlan.Id, CancellationToken.None)).ReturnsAsync(editModel);
            _mealPlanServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<MealPlanEditModel>(), CancellationToken.None))
                .ReturnsAsync((CommandResponse?)null);

            await _viewModel.AddToMealPlanCommand.ExecuteAsync(recipe);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(SaveFailedMessage));
        }

        [Test]
        public async Task AddToMealPlanAsync_ServiceThrows_SetsErrorMessage()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pasta");

            _mealPlanServiceMock
                .Setup(s => s.GetCurrentAsync(CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await _viewModel.AddToMealPlanCommand.ExecuteAsync(recipe);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("boom"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task AddToMealPlanAsync_WhenBusy_DoesNotCallServices()
        {
            var recipe = new RecipeModel(Guid.NewGuid(), "Pasta");
            _viewModel.IsBusy = true;

            await _viewModel.AddToMealPlanCommand.ExecuteAsync(recipe);

            _mealPlanServiceMock.Verify(s => s.GetCurrentAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
