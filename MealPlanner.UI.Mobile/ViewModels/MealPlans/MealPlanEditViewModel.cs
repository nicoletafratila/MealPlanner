using System.Collections.ObjectModel;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.Shared.Resources;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.MealPlans
{
    [QueryProperty(nameof(MealPlanId), "id")]
    public partial class MealPlanEditViewModel(
        IMealPlanService mealPlanService,
        RecipeService recipeService,
        RecipeCategoryService recipeCategoryService,
        IShopService shopService,
        IShoppingListService shoppingListService) : BaseViewModel
    {
        [ObservableProperty]
        private string _mealPlanId = string.Empty;

        [ObservableProperty]
        private MealPlanEditModel _model = new();

        [ObservableProperty]
        private ObservableCollection<RecipeCategoryModel> _categories = [];

        [ObservableProperty]
        private ObservableCollection<RecipeModel> _allRecipes = [];

        [ObservableProperty]
        private ObservableCollection<RecipeModel> _filteredRecipes = [];

        [ObservableProperty]
        private ObservableCollection<ShopModel> _shops = [];

        [ObservableProperty]
        private RecipeCategoryModel? _selectedCategory;

        [ObservableProperty]
        private RecipeModel? _selectedRecipe;

        [ObservableProperty]
        private bool _isNew;

        partial void OnMealPlanIdChanged(string value)
        {
            Guid.TryParse(value, out var id);
            IsNew = id == Guid.Empty;
            _ = LoadAsync();
        }

        partial void OnSelectedCategoryChanged(RecipeCategoryModel? value)
        {
            if (value is null || value.Id == Guid.Empty)
                FilteredRecipes = AllRecipes;
            else
                FilteredRecipes = new ObservableCollection<RecipeModel>(
                    AllRecipes.Where(r => r.RecipeCategoryId == value.Id.ToString()));
            SelectedRecipe = null;
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                var catTask = recipeCategoryService.SearchAsync(new QueryParameters<RecipeCategoryModel> { PageSize = 200, Sorting = DefaultSorting });
                var recipeTask = recipeService.SearchAsync(new QueryParameters<RecipeModel> { PageSize = 500, Sorting = DefaultSorting });
                var shopTask = shopService.SearchAsync(new QueryParameters<ShopModel> { PageSize = 200, Sorting = DefaultSorting });
                await Task.WhenAll(catTask, recipeTask, shopTask);

                if (catTask.Result is not null)
                {
                    var all = new List<RecipeCategoryModel> { new() { Id = Guid.Empty, Name = "All categories" } };
                    all.AddRange(catTask.Result.Items);
                    Categories = new ObservableCollection<RecipeCategoryModel>(all);
                }
                if (recipeTask.Result is not null)
                {
                    AllRecipes = new ObservableCollection<RecipeModel>(recipeTask.Result.Items);
                    FilteredRecipes = AllRecipes;
                }
                if (shopTask.Result is not null)
                    Shops = new ObservableCollection<ShopModel>(shopTask.Result.Items);

                if (!IsNew)
                {
                    Guid.TryParse(MealPlanId, out var id);
                    Model = await mealPlanService.GetEditAsync(id) ?? new();
                }
                Model.Recipes ??= [];
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

        [RelayCommand]
        private void AddRecipe()
        {
            if (SelectedRecipe is null) return;
            Model.Recipes ??= [];
            if (Model.Recipes.Any(r => r.Id == SelectedRecipe.Id)) return;
            Model.Recipes.Add(SelectedRecipe);
            SelectedRecipe = null;
        }

        [RelayCommand]
        private void RemoveRecipe(RecipeModel recipe) => Model.Recipes?.Remove(recipe);

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task SaveAsync()
        {
            if (IsBusy) return; IsBusy = true; ClearMessages();
            try
            {
                var result = IsNew ? await mealPlanService.AddAsync(Model) : await mealPlanService.UpdateAsync(Model);
                if (result?.Succeeded == true) await Shell.Current.GoToAsync("..");
                else SetError(result?.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task DeleteAsync()
        {
            if (IsNew) return;
            var confirm = await Shell.Current.DisplayAlertAsync("Delete", "Delete this meal plan?", "Delete", "Cancel");
            if (!confirm) return;
            IsBusy = true; ClearMessages();
            try
            {
                Guid.TryParse(MealPlanId, out var deleteId);
                var result = await mealPlanService.DeleteAsync(deleteId);
                if (result?.Succeeded == true) await Shell.Current.GoToAsync("..");
                else SetError(result?.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task MakeShoppingListAsync()
        {
            if (IsNew || !(Model.Recipes?.Count > 0)) return;
            if (Shops.Count == 0)
            {
                SetError(MealPlannerSharedMessages.NoShopsAvailable); return;
            }

            var shopNames = Shops.Select(s => s.Name).ToArray();
            var picked = await Shell.Current.DisplayActionSheetAsync("Select shop", "Cancel", null, shopNames);
            if (picked is null || picked == "Cancel") return;

            var shop = Shops.FirstOrDefault(s => s.Name == picked);
            if (shop is null) return;

            IsBusy = true; ClearMessages();
            try
            {
                Guid.TryParse(MealPlanId, out var mealPlanGuid);
                var list = await shoppingListService.MakeShoppingListAsync(new ShoppingListCreateModel { MealPlanId = mealPlanGuid, ShopId = shop.Id });
                if (list is not null)
                    await Shell.Current.GoToAsync($"ShoppingListEdit?id={list.Id}");
                else
                    SetError(MealPlannerSharedMessages.ShoppingListGenerateFailed);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
