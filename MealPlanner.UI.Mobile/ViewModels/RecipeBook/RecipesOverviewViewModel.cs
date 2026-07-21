using System.Collections.ObjectModel;
using Common.Models;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Mobile.Pages.RecipeBook.Resources;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    public partial class RecipesOverviewViewModel(RecipeService recipeService, RecipeCategoryService categoryService, IMealPlanService mealPlanService) : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<RecipeModel> _recipes = [];

        [ObservableProperty]
        private ObservableCollection<RecipeCategoryModel> _categories = [];

        [ObservableProperty]
        private string? _searchText;

        [ObservableProperty]
        private RecipeCategoryModel? _selectedCategory;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private bool _hasNextPage;

        [ObservableProperty]
        private bool _isLoadingMore;

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearMessages();
            try
            {
                var catResult = await categoryService.SearchAsync(new QueryParameters<RecipeCategoryModel> { PageSize = 100 });
                if (catResult is not null)
                    Categories = new ObservableCollection<RecipeCategoryModel>(catResult.Items);

                await SearchAsync();
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task SearchAsync()
        {
            IsBusy = true;
            CurrentPage = 1;
            try
            {
                var filters = BuildFilters();
                var result = await recipeService.SearchAsync(new QueryParameters<RecipeModel> { PageNumber = CurrentPage, PageSize = 20, Filters = filters.Count > 0 ? filters : null, Sorting = DefaultSorting });
                if (result is not null)
                {
                    Recipes = new ObservableCollection<RecipeModel>(result.Items);
                    HasNextPage = result.Metadata.HasNextPage;
                }
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnSearchTextChanged(string? value)
        {
            if (string.IsNullOrEmpty(value))
                SearchCommand.Execute(null);
        }

        [RelayCommand]
        private void ClearCategory() => SelectedCategory = null;

        partial void OnSelectedCategoryChanged(RecipeCategoryModel? value) =>
          SearchCommand.Execute(null);

        [RelayCommand]
        private Task OpenRecipeAsync(RecipeModel recipe) =>
            Shell.Current.GoToAsync($"RecipeDetail?id={recipe.Id}");

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task OpenSourceAsync(string? url)
        {
            if (!string.IsNullOrWhiteSpace(url))
                await Launcher.OpenAsync(new Uri(url));
        }

        [RelayCommand]
        private Task AddRecipeAsync() =>
            Shell.Current.GoToAsync("RecipeEdit?id=0");

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task AddToMealPlanAsync(RecipeModel recipe)
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearMessages();
            try
            {
                var mealPlan = await mealPlanService.GetCurrentAsync();

                if (mealPlan is null)
                {
                    var newPlan = new MealPlanEditModel
                    {
                        Name = mealPlanService.GetMenuName(RecipesOverviewPage.MenuName),
                        Recipes = [recipe]
                    };
                    var createResponse = await mealPlanService.AddAsync(newPlan);
                    if (createResponse is null || !createResponse.Succeeded)
                    {
                        SetError(createResponse?.Message ?? RecipesOverviewPage.SaveFailedMessage);
                        return;
                    }
                    SetSuccess(RecipesOverviewPage.MealPlanCreatedAndRecipeAdded);
                    return;
                }

                var mealPlanToAdd = await mealPlanService.GetEditAsync(mealPlan.Id);
                if (mealPlanToAdd is null)
                {
                    SetError(RecipesOverviewPage.SaveFailedMessage);
                    return;
                }

                mealPlanToAdd.Recipes ??= [];
                mealPlanToAdd.Recipes.Add(recipe);
                mealPlanToAdd.Recipes.SetIndexes();

                var response = await mealPlanService.UpdateAsync(mealPlanToAdd);
                if (response is null || !response.Succeeded)
                {
                    SetError(response?.Message ?? RecipesOverviewPage.SaveFailedMessage);
                    return;
                }

                SetSuccess(RecipesOverviewPage.RecipeAdded);
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task DeleteRecipeAsync(RecipeModel recipe)
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearMessages();
            try
            {
                var result = await recipeService.DeleteAsync(recipe.Id);
                if (result?.Succeeded == true) Recipes.Remove(recipe);
                else SetError(result?.Message);
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task NextPageAsync()
        {
            if (IsLoadingMore || IsBusy || !HasNextPage) return;
            IsLoadingMore = true;
            try
            {
                CurrentPage++;
                await AppendPageAsync();
            }
            finally
            {
                IsLoadingMore = false;
            }
        }

        private async Task AppendPageAsync()
        {
            var filters = BuildFilters();
            var result = await recipeService.SearchAsync(new QueryParameters<RecipeModel> { PageNumber = CurrentPage, PageSize = 20, Filters = filters.Count > 0 ? filters : null, Sorting = DefaultSorting });
            if (result is not null)
            {
                foreach (var item in result.Items)
                    Recipes.Add(item);
                HasNextPage = result.Metadata.HasNextPage;
            }
        }

        private List<FilterItem> BuildFilters()
        {
            var filters = new List<FilterItem>();
            if (!string.IsNullOrWhiteSpace(SearchText))
                filters.Add(new FilterItem("Name", SearchText, FilterOperator.Contains, StringComparison.OrdinalIgnoreCase));
            if (SelectedCategory is not null && SelectedCategory.Id != Guid.Empty)
                filters.Add(new FilterItem("RecipeCategoryId", SelectedCategory.Id.ToString(), FilterOperator.Equals));
            return filters;
        }
    }
}
