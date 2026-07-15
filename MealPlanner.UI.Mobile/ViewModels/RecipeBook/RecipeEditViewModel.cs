using System.Collections.ObjectModel;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;
using RecipeBook.Shared.Resources;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    [QueryProperty(nameof(RecipeId), "id")]
    public partial class RecipeEditViewModel(RecipeService recipeService, RecipeCategoryService categoryService, UnitService unitService, ProductService productService) : BaseViewModel
    {
        [ObservableProperty]
        private string _recipeId = string.Empty;

        [ObservableProperty]
        private RecipeEditModel _model = new();

        [ObservableProperty]
        private ObservableCollection<RecipeCategoryModel> _categories = [];

        [ObservableProperty]
        private ObservableCollection<UnitModel> _units = [];

        [ObservableProperty]
        private ObservableCollection<ProductModel> _products = [];

        [ObservableProperty]
        private ObservableCollection<RecipeIngredientEditViewModel> _recipeIngredients = [];

        [ObservableProperty]
        private RecipeCategoryModel? _selectedCategory;

        [ObservableProperty]
        private ImageSource? _recipeImage;

        [ObservableProperty]
        private bool _isNew;

        partial void OnRecipeIdChanged(string value) => _ = LoadAsync();

        [RelayCommand(AllowConcurrentExecutions = true)]
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
                {
                    Model = await recipeService.GetEditAsync(id) ?? new();
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == Model.RecipeCategoryId);
                    if (Model.ImageContent is { Length: > 0 })
                        RecipeImage = ImageSource.FromStream(() => new MemoryStream(Model.ImageContent));
                    var rows = Model.Ingredients?
                        .Select(i => new RecipeIngredientEditViewModel(i, Products, Units))
                        ?? [];
                    RecipeIngredients = new ObservableCollection<RecipeIngredientEditViewModel>(rows);
                }
                else
                {
                    RecipeIngredients = [];
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

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task PickImageAsync()
        {
            try
            {
                var results = await MediaPicker.Default.PickPhotosAsync();
                var result = results?.FirstOrDefault();
                if (result is null) return;
                await using var stream = await result.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                Model.ImageContent = ms.ToArray();
                RecipeImage = ImageSource.FromStream(() => new MemoryStream(Model.ImageContent));
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task SaveAsync()
        {
            if (IsBusy) return;
            ClearMessages();

            if (SelectedCategory is null)
            {
                SetError(RecipeBookSharedMessages.RecipeCategoryRequired);
                return;
            }

            if (RecipeIngredients.Count == 0)
            {
                SetError(RecipeBookSharedMessages.RecipeRequiresIngredients);
                return;
            }

            if (RecipeIngredients.Any(r => r.Quantity <= 0))
            {
                SetError(RecipeBookSharedMessages.IngredientQuantityPositive);
                return;
            }

            IsBusy = true;
            try
            {
                Model.RecipeCategoryId = SelectedCategory.Id;

                Model.Ingredients = RecipeIngredients.Select(row =>
                {
                    var m = row.Model;
                    m.Quantity = row.Quantity;
                    if (row.SelectedProduct is not null) m.ProductId = row.SelectedProduct.Id;
                    if (row.SelectedUnit is not null) m.UnitId = row.SelectedUnit.Id;
                    return m;
                }).ToList();

                var result = IsNew
                    ? await recipeService.AddAsync(Model)
                    : await recipeService.UpdateAsync(Model);
                if (result?.Succeeded == true) await Shell.Current.GoToAsync("..");
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

        [RelayCommand]
        private void AddIngredient() =>
            RecipeIngredients.Add(new RecipeIngredientEditViewModel(Products, Units));

        [RelayCommand]
        private void RemoveIngredient(RecipeIngredientEditViewModel row) =>
            RecipeIngredients.Remove(row);
    }
}
