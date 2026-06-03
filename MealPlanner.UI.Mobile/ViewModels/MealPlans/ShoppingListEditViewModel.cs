using System.Collections.ObjectModel;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.MealPlans
{
    [QueryProperty(nameof(ShoppingListId), "id")]
    public partial class ShoppingListEditViewModel(
        IShoppingListService shoppingListService,
        IShopService shopService,
        ProductCategoryService productCategoryService,
        ProductService productService,
        UnitService unitService,
        IMealPlanService mealPlanService,
        RecipeService recipeService,
        RecipeCategoryService recipeCategoryService) : BaseViewModel
    {
        [ObservableProperty] private int _shoppingListId;
        [ObservableProperty] private ShoppingListEditModel _model = new();
        [ObservableProperty] private bool _isNew;

        // Shop selection
        [ObservableProperty] private ObservableCollection<ShopModel> _shops = [];
        [ObservableProperty] private ShopModel? _selectedShop;

        // Product add
        [ObservableProperty] private ObservableCollection<ProductCategoryModel> _productCategories = [];
        [ObservableProperty] private ProductCategoryModel? _selectedProductCategory;
        [ObservableProperty] private ObservableCollection<ProductModel> _productsByCategory = [];
        [ObservableProperty] private ProductModel? _selectedProduct;
        [ObservableProperty] private ObservableCollection<UnitModel> _unitsForProduct = [];
        [ObservableProperty] private UnitModel? _selectedUnit;
        [ObservableProperty] private string _quantityText = string.Empty;

        // All units for "add from meal plan/recipe" merging
        private IList<UnitModel> _allUnits = [];

        partial void OnShoppingListIdChanged(int value) { IsNew = value == 0; _ = LoadAsync(); }

        partial void OnSelectedShopChanged(ShopModel? value) { if (value is not null) Model.ShopId = value.Id; }

        partial void OnSelectedProductCategoryChanged(ProductCategoryModel? value) =>
            _ = LoadProductsByCategoryAsync(value?.Id);

        partial void OnSelectedProductChanged(ProductModel? value) => RefreshUnitsForProduct(value);

        [RelayCommand]
        public async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                var shopTask = shopService.SearchAsync(new QueryParameters<ShopModel> { PageSize = 200, Sorting = DefaultSorting });
                var catTask = productCategoryService.SearchAsync(new QueryParameters<ProductCategoryModel> { PageSize = 200, Sorting = DefaultSorting });
                var unitTask = unitService.SearchAsync(new QueryParameters<UnitModel> { PageSize = 200, Sorting = DefaultSorting });
                await Task.WhenAll(shopTask, catTask, unitTask);

                if (shopTask.Result is not null) Shops = new ObservableCollection<ShopModel>(shopTask.Result.Items);
                if (catTask.Result is not null)
                {
                    var cats = new List<ProductCategoryModel> { new() { Id = 0, Name = "All categories" } };
                    cats.AddRange(catTask.Result.Items);
                    ProductCategories = new ObservableCollection<ProductCategoryModel>(cats);
                }
                if (unitTask.Result is not null) _allUnits = unitTask.Result.Items;

                if (!IsNew)
                {
                    Model = await shoppingListService.GetEditAsync(ShoppingListId) ?? new();
                    Model.Products ??= [];
                    SelectedShop = Shops.FirstOrDefault(s => s.Id == Model.ShopId);
                }
                else
                {
                    Model.Products ??= [];
                }
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        private async Task LoadProductsByCategoryAsync(int? categoryId)
        {
            try
            {
                var result = categoryId is null or 0
                    ? await productService.SearchAsync(new QueryParameters<ProductModel> { PageSize = 500, Sorting = DefaultSorting })
                    : await productService.SearchAsync(new QueryParameters<ProductModel>
                    {
                        PageSize = 500,
                        Sorting = DefaultSorting,
                        Filters = [new FilterItem("ProductCategoryId", categoryId.ToString(), FilterOperator.Equals)]
                    });
                ProductsByCategory = result is not null
                    ? new ObservableCollection<ProductModel>(result.Items)
                    : [];
                SelectedProduct = null;
            }
            catch { /* ignore */ }
        }

        private void RefreshUnitsForProduct(ProductModel? product)
        {
            if (product?.BaseUnit is null)
            {
                UnitsForProduct = new ObservableCollection<UnitModel>(_allUnits);
            }
            else
            {
                var type = product.BaseUnit.UnitType;
                UnitsForProduct = new ObservableCollection<UnitModel>(
                    _allUnits.Where(u => u.UnitType == type));
            }
            SelectedUnit = UnitsForProduct.FirstOrDefault(u => u.Id == product?.BaseUnit?.Id);
            QuantityText = string.Empty;
        }

        [RelayCommand]
        private void AddProduct()
        {
            if (SelectedProduct is null || SelectedUnit is null) return;
            if (!decimal.TryParse(QuantityText, out var qty) || qty <= 0) return;

            Model.Products ??= [];
            var existing = Model.Products.FirstOrDefault(p => p.Product?.Id == SelectedProduct.Id);
            if (existing is not null)
            {
                existing.Quantity += qty;
            }
            else
            {
                Model.Products.Add(new ShoppingListProductEditModel
                {
                    ShoppingListId = Model.Id,
                    Product = SelectedProduct,
                    Quantity = qty,
                    UnitId = SelectedUnit.Id,
                    Unit = SelectedUnit,
                    Collected = false,
                    DisplaySequence = Model.Products.Count + 1
                });
            }

            QuantityText = string.Empty;
            SelectedProduct = null;
        }

        [RelayCommand]
        private async Task AddFromMealPlanAsync()
        {
            if (Model.ShopId == Guid.Empty) { SetError("Select a shop first."); return; }

            var plans = await mealPlanService.SearchAsync(new QueryParameters<MealPlanModel> { PageSize = 200, Sorting = DefaultSorting });
            if (plans is null || plans.Items.Count == 0) { SetError("No meal plans found."); return; }

            var names = plans.Items.Select(p => p.Name).ToArray();
            var picked = await Shell.Current.DisplayActionSheetAsync("Select meal plan", "Cancel", null, names);
            if (picked is null || picked == "Cancel") return;

            var plan = plans.Items.FirstOrDefault(p => p.Name == picked);
            if (plan is null) return;

            IsBusy = true;
            try
            {
                var products = await mealPlanService.GetShoppingListProductsAsync(plan.Id, Model.ShopId);
                if (products is not null) MergeProducts(products);
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task AddFromRecipeAsync()
        {
            if (Model.ShopId == Guid.Empty) { SetError("Select a shop first."); return; }

            var categories = await recipeCategoryService.SearchAsync(new QueryParameters<RecipeCategoryModel> { PageSize = 200, Sorting = DefaultSorting });
            string[]? catNames = categories?.Items?.Select(c => c.Name).ToArray();
            int? recipeCategoryId = null;

            if (catNames is { Length: > 0 })
            {
                var pickedCat = await Shell.Current.DisplayActionSheetAsync("Filter by category (optional)", "Skip", null, catNames);
                if (pickedCat is not null && pickedCat != "Skip")
                    recipeCategoryId = categories!.Items.FirstOrDefault(c => c.Name == pickedCat)?.Id;
            }

            var recipes = await recipeService.SearchAsync(new QueryParameters<RecipeModel>
            {
                PageSize = 500,
                Sorting = DefaultSorting,
                Filters = recipeCategoryId.HasValue ? [new FilterItem("RecipeCategoryId", recipeCategoryId.ToString(), FilterOperator.Equals)] : null
            });
            if (recipes is null || recipes.Items.Count == 0) { SetError("No recipes found."); return; }

            var names = recipes.Items.Select(r => r.Name).ToArray();
            var picked = await Shell.Current.DisplayActionSheetAsync("Select recipe", "Cancel", null, names);
            if (picked is null || picked == "Cancel") return;

            var recipe = recipes.Items.FirstOrDefault(r => r.Name == picked);
            if (recipe is null) return;

            IsBusy = true;
            try
            {
                var products = await recipeService.GetShoppingListProductsAsync(recipe.Id, Model.ShopId);
                if (products is not null) MergeProducts(products);
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        private void MergeProducts(IList<ShoppingListProductEditModel> incoming)
        {
            Model.Products ??= [];
            foreach (var item in incoming)
            {
                var existing = Model.Products.FirstOrDefault(p => p.Product?.Id == item.Product?.Id);
                if (existing is not null)
                    existing.Quantity += item.Quantity;
                else
                {
                    item.ShoppingListId = Model.Id;
                    Model.Products.Add(item);
                }
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (IsBusy) return; IsBusy = true; ClearMessages();
            try
            {
                var result = IsNew ? await shoppingListService.AddAsync(Model) : await shoppingListService.UpdateAsync(Model);
                if (result?.Succeeded == true) await Shell.Current.GoToAsync("..");
                else SetError(result?.Message);
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (IsNew) return;
            var confirm = await Shell.Current.DisplayAlertAsync("Delete", "Delete this shopping list?", "Delete", "Cancel");
            if (!confirm) return;
            IsBusy = true; ClearMessages();
            try
            {
                var result = await shoppingListService.DeleteAsync(ShoppingListId);
                if (result?.Succeeded == true) await Shell.Current.GoToAsync("..");
                else SetError(result?.Message);
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task ExportAsync()
        {
            if (Model.Products is not { Count: > 0 }) return;
            var text = string.Join(Environment.NewLine,
                Model.Products.Select(p => $"{p.Product?.Name} - {p.Quantity} {p.Unit?.Name}"));
            await Clipboard.SetTextAsync(text);
            await Shell.Current.DisplayAlertAsync("Exported", "Shopping list copied to clipboard.", "OK");
        }

        [RelayCommand]
        private void ToggleProductCollected(ShoppingListProductEditModel product) =>
            product.Collected = !product.Collected;

        [RelayCommand]
        private void RemoveProduct(ShoppingListProductEditModel product) =>
            Model.Products?.Remove(product);
    }
}
