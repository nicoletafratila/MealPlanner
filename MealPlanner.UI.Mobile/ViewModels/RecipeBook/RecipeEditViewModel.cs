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
    public partial class RecipeEditViewModel(
        RecipeService recipeService,
        RecipeCategoryService categoryService,
        UnitService unitService,
        ProductService productService,
        ProductCategoryService productCategoryService) : BaseViewModel
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

        partial void OnRecipeIdChanged(string value) => _ = LoadAsync();

        partial void OnSelectedProductCategoryChanged(ProductCategoryModel? value) => RefreshProductsByCategory(value);

        partial void OnSelectedProductChanged(ProductModel? value) => RefreshUnitsForProduct(value);

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
                var prodCatTask = productCategoryService.SearchAsync(new QueryParameters<ProductCategoryModel> { PageSize = 200, Sorting = DefaultSorting });
                await Task.WhenAll(catTask, unitTask, prodTask, prodCatTask);

                if (catTask.Result is not null) Categories = new ObservableCollection<RecipeCategoryModel>(catTask.Result.Items);
                if (unitTask.Result is not null) Units = new ObservableCollection<UnitModel>(unitTask.Result.Items);
                if (prodTask.Result is not null) Products = new ObservableCollection<ProductModel>(prodTask.Result.Items);
                if (prodCatTask.Result is not null)
                {
                    var prodCats = new List<ProductCategoryModel> { new() { Id = Guid.Empty, Name = Pages.RecipeBook.Resources.RecipeEditPage.AllCategoriesOption } };
                    prodCats.AddRange(prodCatTask.Result.Items);
                    ProductCategories = new ObservableCollection<ProductCategoryModel>(prodCats);
                }

                if (!IsNew)
                {
                    Model = await recipeService.GetEditAsync(id) ?? new();
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

            RecipeIngredients = new ObservableCollection<RecipeIngredientEditModel>(ordered);
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
