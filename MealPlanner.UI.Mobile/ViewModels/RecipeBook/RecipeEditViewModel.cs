using System.Collections.ObjectModel;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    [QueryProperty(nameof(RecipeId), "id")]
    public partial class RecipeEditViewModel(RecipeService recipeService, RecipeCategoryService categoryService, UnitService unitService, ProductService productService) : BaseViewModel
    {
        [ObservableProperty] private string _recipeId = string.Empty;
        [ObservableProperty] private RecipeEditModel _model = new();
        [ObservableProperty] private ObservableCollection<RecipeCategoryModel> _categories = [];
        [ObservableProperty] private ObservableCollection<UnitModel> _units = [];
        [ObservableProperty] private ObservableCollection<ProductModel> _products = [];
        [ObservableProperty] private bool _isNew;

        partial void OnRecipeIdChanged(string value) => _ = LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            IsBusy = true;
            Guid.TryParse(RecipeId, out var id);
            IsNew = id == Guid.Empty;
            try
            {
                var catTask = categoryService.SearchAsync(new QueryParameters<RecipeCategoryModel> { PageSize = 100, Sorting = DefaultSorting });
                var unitTask = unitService.SearchAsync(new QueryParameters<UnitModel> { PageSize = 100, Sorting = DefaultSorting });
                var prodTask = productService.SearchAsync(new QueryParameters<ProductModel> { PageSize = 500, Sorting = DefaultSorting });
                await Task.WhenAll(catTask, unitTask, prodTask);

                if (catTask.Result is not null) Categories = new ObservableCollection<RecipeCategoryModel>(catTask.Result.Items);
                if (unitTask.Result is not null) Units = new ObservableCollection<UnitModel>(unitTask.Result.Items);
                if (prodTask.Result is not null) Products = new ObservableCollection<ProductModel>(prodTask.Result.Items);

                if (!IsNew)
                    Model = await recipeService.GetEditAsync(id) ?? new();
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
        private async Task SaveAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearMessages();
            try
            {
                var result = IsNew
                    ? await recipeService.AddAsync(Model)
                    : await recipeService.UpdateAsync(Model);
                if (result?.Succeeded == true) await Shell.Current.GoToAsync("..");
                else SetError(result?.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void AddIngredient()
        {
            Model.Ingredients ??= [];
            Model.Ingredients.Add(new RecipeIngredientEditModel());
        }

        [RelayCommand]
        private void RemoveIngredient(RecipeIngredientEditModel ingredient) =>
            Model.Ingredients?.Remove(ingredient);
    }
}
