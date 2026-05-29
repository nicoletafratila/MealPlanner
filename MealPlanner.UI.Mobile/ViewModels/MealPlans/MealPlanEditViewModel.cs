using System.Collections.ObjectModel;using Common.Pagination; using CommunityToolkit.Mvvm.ComponentModel; using CommunityToolkit.Mvvm.Input; using MealPlanner.Services.Core.Http; using MealPlanner.Shared.Models; using RecipeBook.Services.Core.Http; using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.MealPlans
{
    [QueryProperty(nameof(MealPlanId), "id")]
    public partial class MealPlanEditViewModel(
        MealPlanService mealPlanService,
        RecipeService recipeService,
        RecipeCategoryService recipeCategoryService,
        ShopService shopService,
        ShoppingListService shoppingListService) : BaseViewModel
    {
        [ObservableProperty] private int _mealPlanId;
        [ObservableProperty] private MealPlanEditModel _model = new();
        [ObservableProperty] private ObservableCollection<RecipeCategoryModel> _categories = [];
        [ObservableProperty] private ObservableCollection<RecipeModel> _allRecipes = [];
        [ObservableProperty] private ObservableCollection<RecipeModel> _filteredRecipes = [];
        [ObservableProperty] private ObservableCollection<ShopModel> _shops = [];
        [ObservableProperty] private RecipeCategoryModel? _selectedCategory;
        [ObservableProperty] private RecipeModel? _selectedRecipe;
        [ObservableProperty] private bool _isNew;

        partial void OnMealPlanIdChanged(int value) { IsNew = value == 0; _ = LoadAsync(); }

        partial void OnSelectedCategoryChanged(RecipeCategoryModel? value)
        {
            if (value is null || value.Id == 0)
                FilteredRecipes = AllRecipes;
            else
                FilteredRecipes = new ObservableCollection<RecipeModel>(
                    AllRecipes.Where(r => r.RecipeCategoryId == value.Id.ToString()));
            SelectedRecipe = null;
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                var catTask = recipeCategoryService.SearchAsync(new QueryParameters<RecipeCategoryModel> { PageSize = 200 });
                var recipeTask = recipeService.SearchAsync(new QueryParameters<RecipeModel> { PageSize = 500 });
                var shopTask = shopService.SearchAsync(new QueryParameters<ShopModel> { PageSize = 200 });
                await Task.WhenAll(catTask, recipeTask, shopTask);

                if (catTask.Result is not null)
                {
                    var all = new List<RecipeCategoryModel> { new() { Id = 0, Name = "All categories" } };
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
                    Model = await mealPlanService.GetEditAsync(MealPlanId) ?? new();
                Model.Recipes ??= [];
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
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

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (IsBusy) return; IsBusy = true; ClearMessages();
            try
            {
                var (success, error) = IsNew ? await mealPlanService.AddAsync(Model) : await mealPlanService.UpdateAsync(Model);
                if (success) await Shell.Current.GoToAsync("..");
                else SetError(error);
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (IsNew) return;
            var confirm = await Shell.Current.DisplayAlert("Delete", "Delete this meal plan?", "Delete", "Cancel");
            if (!confirm) return;
            IsBusy = true; ClearMessages();
            try
            {
                var (success, error) = await mealPlanService.DeleteAsync(MealPlanId);
                if (success) await Shell.Current.GoToAsync("..");
                else SetError(error);
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task MakeShoppingListAsync()
        {
            if (IsNew || !(Model.Recipes?.Count > 0)) return;
            if (Shops.Count == 0) { SetError("No shops available."); return; }

            var shopNames = Shops.Select(s => s.Name).ToArray();
            var picked = await Shell.Current.DisplayActionSheet("Select shop", "Cancel", null, shopNames);
            if (picked is null || picked == "Cancel") return;

            var shop = Shops.FirstOrDefault(s => s.Name == picked);
            if (shop is null) return;

            IsBusy = true; ClearMessages();
            try
            {
                var list = await shoppingListService.GenerateAsync(MealPlanId, shop.Id);
                if (list is not null)
                    await Shell.Current.GoToAsync($"ShoppingListEdit?id={list.Id}");
                else
                    SetError("Failed to generate shopping list.");
            }
            finally { IsBusy = false; }
        }
    }
}
