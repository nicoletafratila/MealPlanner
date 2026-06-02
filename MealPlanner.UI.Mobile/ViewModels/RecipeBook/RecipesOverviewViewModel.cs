using System.Collections.ObjectModel;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    public partial class RecipesOverviewViewModel(RecipeService recipeService, RecipeCategoryService categoryService) : BaseViewModel
    {
        [ObservableProperty] private ObservableCollection<RecipeModel> _recipes = [];
        [ObservableProperty] private ObservableCollection<RecipeCategoryModel> _categories = [];
        [ObservableProperty] private string? _searchText;
        [ObservableProperty] private RecipeCategoryModel? _selectedCategory;
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private bool _hasNextPage;

        [RelayCommand]
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
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            IsBusy = true;
            CurrentPage = 1;
            try
            {
                var filters = new List<FilterItem>();
                if (!string.IsNullOrWhiteSpace(SearchText))
                    filters.Add(new FilterItem("Name", SearchText, FilterOperator.Contains, StringComparison.OrdinalIgnoreCase));
                if (SelectedCategory?.Id > 0)
                    filters.Add(new FilterItem("RecipeCategoryId", SelectedCategory.Id.ToString(), FilterOperator.Equals));
                var result = await recipeService.SearchAsync(new QueryParameters<RecipeModel> { PageNumber = CurrentPage, PageSize = 20, Filters = filters.Count > 0 ? filters : null, Sorting = DefaultSorting });
                if (result is not null)
                {
                    Recipes = new ObservableCollection<RecipeModel>(result.Items);
                    HasNextPage = result.Metadata.HasNextPage;
                }
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private Task OpenRecipeAsync(RecipeModel recipe) =>
            Shell.Current.GoToAsync($"RecipeDetail?id={recipe.Id}");

        [RelayCommand]
        private Task AddRecipeAsync() =>
            Shell.Current.GoToAsync("RecipeEdit?id=0");

        [RelayCommand]
        private async Task DeleteRecipeAsync(RecipeModel recipe)
        {
            var result = await recipeService.DeleteAsync(recipe.Id);
            if (result?.Succeeded == true) Recipes.Remove(recipe);
            else SetError(result?.Message);
        }

        [RelayCommand] private async Task NextPageAsync() { CurrentPage++; await RefreshPageAsync(); }
        private async Task RefreshPageAsync()
        {
            var filters = new List<FilterItem>();
            if (!string.IsNullOrWhiteSpace(SearchText))
                filters.Add(new FilterItem("Name", SearchText, FilterOperator.Contains, StringComparison.OrdinalIgnoreCase));
            if (SelectedCategory?.Id > 0)
                filters.Add(new FilterItem("RecipeCategoryId", SelectedCategory.Id.ToString(), FilterOperator.Equals));
            var result = await recipeService.SearchAsync(new QueryParameters<RecipeModel> { PageNumber = CurrentPage, PageSize = 20, Filters = filters.Count > 0 ? filters : null, Sorting = DefaultSorting });
            if (result is not null)
            {
                Recipes = new ObservableCollection<RecipeModel>(result.Items);
                HasNextPage = result.Metadata.HasNextPage;
            }
        }
    }
}
