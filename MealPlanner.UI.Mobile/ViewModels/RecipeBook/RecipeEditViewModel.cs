using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MealPlanner.UI.Mobile.Extensions;
using MealPlanner.UI.Mobile.Services;
using Microsoft.Maui.Graphics.Platform;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;
using RecipeBook.Shared.Resources;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    public partial class RecipeEditViewModel(
        IRecipeService recipeService,
        ReferenceDataCacheService lookupDataService) : BaseViewModel, IQueryAttributable
    {
        // Recipe search returns every recipe's image inline for list thumbnails, so keeping
        // stored images small keeps that endpoint fast for users with many recipes.
        private const float MaxImageDimension = 1024;
        private const float ImageQuality = 0.8f;

        private RecipeEditModel? _preloadedModel;
        private bool _hasLoaded;

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
        private ObservableCollection<RecipeIngredientEditModel> _recipeIngredients = [];

        [ObservableProperty]
        private RecipeCategoryModel? _selectedCategory;

        [ObservableProperty]
        private ImageSource? _recipeImage;

        [ObservableProperty]
        private bool _isNew;

        // Product category filter
        [ObservableProperty]
        private ObservableCollection<ProductCategoryModel> _productCategories = [];

        [ObservableProperty]
        private ProductCategoryModel? _selectedProductCategory;

        [ObservableProperty]
        private ObservableCollection<ProductModel> _productsByCategory = [];

        // Add ingredient section
        [ObservableProperty]
        private ProductModel? _selectedProduct;

        [ObservableProperty]
        private ObservableCollection<UnitModel> _unitsForProduct = [];

        [ObservableProperty]
        private UnitModel? _selectedUnit;

        [ObservableProperty]
        private string _quantityText = string.Empty;

        partial void OnSelectedProductCategoryChanged(ProductCategoryModel? value) => RefreshProductsByCategory(value);

        partial void OnSelectedProductChanged(ProductModel? value) => RefreshUnitsForProduct(value);

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (_hasLoaded) return;
            _hasLoaded = true;

            if (query.TryGetValue("model", out var modelObj) && modelObj is RecipeEditModel preloaded)
                _preloadedModel = preloaded;

            if (query.TryGetValue("id", out var idObj))
                RecipeId = idObj?.ToString() ?? string.Empty;

            _ = LoadAsync();
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            Guid.TryParse(RecipeId, out var id);
            IsNew = id == Guid.Empty;
            try
            {
                await Task.WhenAll(lookupDataService.EnsureLoadedAsync(), lookupDataService.EnsureProductsLoadedAsync());
                Categories = lookupDataService.Categories;
                Units = lookupDataService.Units;
                Products = lookupDataService.Products;
                ProductCategories = WithAllCategoriesOption(lookupDataService.ProductCategories);

                if (!IsNew)
                {
                    Model = _preloadedModel is { } pre && pre.Id == id ? pre : await recipeService.GetEditAsync(id) ?? new();
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == Model.RecipeCategoryId);
                    if (Model.ImageContent is { Length: > 0 })
                        RecipeImage = ImageSource.FromStream(() => new MemoryStream(Model.ImageContent));
                    Model.Ingredients ??= [];
                }
                else
                {
                    Model.Ingredients ??= [];
                }

                RecipeIngredients = new ObservableCollection<RecipeIngredientEditModel>(Model.Ingredients);
                SortIngredientsByCategory();
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

        private static ObservableCollection<ProductCategoryModel> WithAllCategoriesOption(IEnumerable<ProductCategoryModel> categories)
        {
            var list = new List<ProductCategoryModel> { new() { Id = Guid.Empty, Name = Pages.RecipeBook.Resources.RecipeEditPage.AllCategoriesOption } };
            list.AddRange(categories);
            return new ObservableCollection<ProductCategoryModel>(list);
        }

        private void SortIngredientsByCategory()
        {
            var categoryOrder = ProductCategories
                .Select((c, index) => new { c.Id, Index = index })
                .ToDictionary(x => x.Id, x => x.Index);

            int OrderOf(RecipeIngredientEditModel ingredient)
            {
                var categoryId = ingredient.Product?.ProductCategory?.Id;
                return categoryId is not null && categoryOrder.TryGetValue(categoryId.Value, out var index)
                    ? index
                    : int.MaxValue;
            }

            var ordered = RecipeIngredients
                .OrderBy(OrderOf)
                .ThenBy(i => i.Product?.Name)
                .ToList();

            RecipeIngredients.Replace(ordered);
        }

        private void RefreshProductsByCategory(ProductCategoryModel? category)
        {
            ProductsByCategory = category is null || category.Id == Guid.Empty
                ? new ObservableCollection<ProductModel>(Products)
                : new ObservableCollection<ProductModel>(Products.Where(p => p.ProductCategory?.Id == category.Id));
            SelectedProduct = null;
        }

        private void RefreshUnitsForProduct(ProductModel? product)
        {
            UnitsForProduct = product?.BaseUnit is null
                ? new ObservableCollection<UnitModel>(Units)
                : new ObservableCollection<UnitModel>(Units.Where(u => u.UnitType == product.BaseUnit.UnitType));
            SelectedUnit = UnitsForProduct.FirstOrDefault(u => u.Id == product?.BaseUnit?.Id);
            QuantityText = string.Empty;
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
                Model.ImageContent = await ResizeImageAsync(stream);
                RecipeImage = ImageSource.FromStream(() => new MemoryStream(Model.ImageContent));
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
        }

        private static Task<byte[]> ResizeImageAsync(Stream stream) => Task.Run(() =>
        {
            using var image = PlatformImage.FromStream(stream);
            using var resized = image.Downsize(MaxImageDimension, disposeOriginal: false);
            using var output = new MemoryStream();
            resized.Save(output, ImageFormat.Jpeg, ImageQuality);
            return output.ToArray();
        });

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task SaveAsync()
        {
            if (IsBusy) return;
            ClearMessages();

            if (string.IsNullOrWhiteSpace(Model.Name))
            {
                SetError(RecipeBookSharedMessages.RecipeNameRequired);
                return;
            }

            if (Model.ImageContent is not { Length: > 0 })
            {
                SetError(RecipeBookSharedMessages.ImageRequired);
                return;
            }

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

            if (RecipeIngredients.Any(i => i.Quantity <= 0))
            {
                SetError(RecipeBookSharedMessages.IngredientQuantityPositive);
                return;
            }

            IsBusy = true;
            try
            {
                Model.RecipeCategoryId = SelectedCategory.Id;
                Model.Ingredients = RecipeIngredients.ToList();

                var result = IsNew
                    ? await recipeService.AddAsync(Model)
                    : await recipeService.UpdateAsync(Model);
                if (result?.Succeeded == true)
                {
                    lookupDataService.InvalidateRecipes();
                    await Shell.Current.GoToAsync("..");
                }
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
        private void AddIngredient()
        {
            if (SelectedProduct is null || SelectedUnit is null) return;
            if (!decimal.TryParse(QuantityText, out var qty) || qty <= 0) return;

            var existing = RecipeIngredients.FirstOrDefault(i => i.Product?.Id == SelectedProduct.Id);
            if (existing is not null)
            {
                existing.Quantity += qty;
                var index = RecipeIngredients.IndexOf(existing);
                RecipeIngredients[index] = existing;
            }
            else
            {
                RecipeIngredients.Add(new RecipeIngredientEditModel
                {
                    RecipeId = Model.Id,
                    Product = SelectedProduct,
                    ProductId = SelectedProduct.Id,
                    Quantity = qty,
                    UnitId = SelectedUnit.Id,
                    Unit = SelectedUnit
                });
            }

            SortIngredientsByCategory();
            QuantityText = string.Empty;
            SelectedProduct = null;
        }

        [RelayCommand]
        private void RemoveIngredient(RecipeIngredientEditModel ingredient) =>
            RecipeIngredients.Remove(ingredient);
    }
}
