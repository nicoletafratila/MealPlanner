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
    [QueryProperty(nameof(ShoppingListId), "id")]
    public partial class ShoppingListEditViewModel(
        IShoppingListService shoppingListService,
        IShopService shopService,
        ProductCategoryService productCategoryService,
        ProductService productService,
        UnitService unitService,
        IMealPlanService mealPlanService,
        RecipeService recipeService) : BaseViewModel
    {
        [ObservableProperty]
        private string _shoppingListId = string.Empty;

        [ObservableProperty]
        private ShoppingListEditModel _model = new();

        [ObservableProperty]
        private bool _isNew;

        // Shop selection
        [ObservableProperty]
        private ObservableCollection<ShopModel> _shops = [];

        [ObservableProperty]
        private ShopModel? _selectedShop;

        // Product add
        [ObservableProperty]
        private ObservableCollection<ProductCategoryModel> _productCategories = [];

        [ObservableProperty]
        private ProductCategoryModel? _selectedProductCategory;

        [ObservableProperty]
        private ObservableCollection<ProductModel> _productsByCategory = [];

        [ObservableProperty]
        private ProductModel? _selectedProduct;

        [ObservableProperty]
        private ObservableCollection<UnitModel> _unitsForProduct = [];

        [ObservableProperty]
        private UnitModel? _selectedUnit;

        [ObservableProperty]
        private string _quantityText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ShoppingListProductEditModel> _shoppingListProducts = [];

        [ObservableProperty]
        private bool _isEditingExpanded = true;

        // All units for "add from meal plan/recipe" merging
        private IList<UnitModel> _allUnits = [];

        partial void OnShoppingListIdChanged(string value)
        {
            Guid.TryParse(value, out var id);
            IsNew = id == Guid.Empty;
            _ = LoadAsync();
        }

        partial void OnSelectedShopChanged(ShopModel? value)
        {
            if (value is not null) Model.ShopId = value.Id;
        }

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
                    var cats = new List<ProductCategoryModel> { new() { Id = Guid.Empty, Name = "All categories" } };
                    cats.AddRange(catTask.Result.Items);
                    ProductCategories = new ObservableCollection<ProductCategoryModel>(cats);
                }
                if (unitTask.Result is not null) _allUnits = unitTask.Result.Items;

                if (!IsNew)
                {
                    Guid.TryParse(ShoppingListId, out var id);
                    Model = await shoppingListService.GetEditAsync(id) ?? new();
                    Model.Products ??= [];
                    SelectedShop = Shops.FirstOrDefault(s => s.Id == Model.ShopId);
                }
                else
                {
                    Model.Products ??= [];
                }

                ShoppingListProducts = new ObservableCollection<ShoppingListProductEditModel>(Model.Products);
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

        private async Task LoadProductsByCategoryAsync(Guid? categoryId)
        {
            try
            {
                var result = categoryId is null || categoryId.Value == Guid.Empty
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
        private void ToggleEditing() => IsEditingExpanded = !IsEditingExpanded;

        [RelayCommand]
        private void AddProduct()
        {
            if (SelectedProduct is null || SelectedUnit is null) return;
            if (!decimal.TryParse(QuantityText, out var qty) || qty <= 0) return;

            var existing = ShoppingListProducts.FirstOrDefault(p => p.Product?.Id == SelectedProduct.Id);
            if (existing is not null)
            {
                existing.Quantity += qty;
                var index = ShoppingListProducts.IndexOf(existing);
                ShoppingListProducts[index] = existing;
            }
            else
            {
                ShoppingListProducts.Add(new ShoppingListProductEditModel
                {
                    ShoppingListId = Model.Id,
                    Product = SelectedProduct,
                    Quantity = qty,
                    UnitId = SelectedUnit.Id,
                    Unit = SelectedUnit,
                    Collected = false,
                    DisplaySequence = ShoppingListProducts.Count + 1
                });
            }

            QuantityText = string.Empty;
            SelectedProduct = null;
        }

        public async Task<IReadOnlyList<MealPlanModel>?> LoadMealPlansForSelectionAsync()
        {
            if (Model.ShopId == Guid.Empty)
            {
                SetError(MealPlannerSharedMessages.SelectShopFirst);
                return null;
            }

            try
            {
                var plans = await mealPlanService.SearchAsync(new QueryParameters<MealPlanModel> { PageSize = 200, Sorting = DefaultSorting });
                if (plans is null || plans.Items.Count == 0)
                {
                    SetError(MealPlannerSharedMessages.NoMealPlansFound);
                    return null;
                }
                return plans.Items;
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
                return null;
            }
        }

        public async Task AddFromMealPlanAsync(MealPlanModel plan)
        {
            IsBusy = true;
            try
            {
                var products = await mealPlanService.GetShoppingListProductsAsync(plan.Id, Model.ShopId);
                if (products is not null) MergeProducts(products);
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

        public async Task<IReadOnlyList<RecipeModel>?> LoadRecipesForSelectionAsync()
        {
            if (Model.ShopId == Guid.Empty)
            {
                SetError(MealPlannerSharedMessages.SelectShopFirst);
                return null;
            }

            try
            {
                var recipes = await recipeService.SearchAsync(new QueryParameters<RecipeModel> { PageSize = 500, Sorting = DefaultSorting });
                if (recipes is null || recipes.Items.Count == 0)
                {
                    SetError(MealPlannerSharedMessages.NoRecipesFound);
                    return null;
                }
                return recipes.Items;
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
                return null;
            }
        }

        public async Task AddFromRecipeAsync(RecipeModel recipe)
        {
            IsBusy = true;
            try
            {
                var products = await recipeService.GetShoppingListProductsAsync(recipe.Id, Model.ShopId);
                if (products is not null) MergeProducts(products);
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

        private void MergeProducts(IList<ShoppingListProductEditModel> incoming)
        {
            foreach (var item in incoming)
            {
                var existing = ShoppingListProducts.FirstOrDefault(p => p.Product?.Id == item.Product?.Id);
                if (existing is not null)
                {
                    existing.Quantity += item.Quantity;
                    var index = ShoppingListProducts.IndexOf(existing);
                    ShoppingListProducts[index] = existing;
                }
                else
                {
                    item.ShoppingListId = Model.Id;
                    ShoppingListProducts.Add(item);
                }
            }
        }

        public async Task OnProductCollectedChangedAsync(ShoppingListProductEditModel item)
        {
            ReorderProduct(item);
            await PersistProductsAsync();
        }

        private void ReorderProduct(ShoppingListProductEditModel item)
        {
            var list = ShoppingListProducts.ToList();
            var oldIndex = list.IndexOf(item);
            if (oldIndex < 0) return;

            list.RemoveAt(oldIndex);

            var newIndex = item.Collected
                ? list.Count
                : Math.Max(0, list.Count(p => !p.Collected));

            list.Insert(newIndex, item);

            for (var i = 0; i < list.Count; i++)
                list[i].DisplaySequence = i + 1;

            ShoppingListProducts = new ObservableCollection<ShoppingListProductEditModel>(list);
        }

        private async Task PersistProductsAsync()
        {
            if (IsNew) return;

            try
            {
                Model.Products = ShoppingListProducts.ToList();
                var result = await shoppingListService.UpdateAsync(Model);
                if (result?.Succeeded != true) SetError(result?.Message);
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
                SetError(MealPlannerSharedMessages.ShoppingListNameRequired);
                return;
            }

            if (Model.ShopId == Guid.Empty)
            {
                SetError(MealPlannerSharedMessages.SelectShopFirst);
                return;
            }

            if (ShoppingListProducts.Count == 0)
            {
                SetError(MealPlannerSharedMessages.ShoppingListRequiresProducts);
                return;
            }

            IsBusy = true;
            try
            {
                Model.Products = ShoppingListProducts.ToList();
                var result = IsNew ? await shoppingListService.AddAsync(Model) : await shoppingListService.UpdateAsync(Model);
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

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task DeleteAsync()
        {
            if (IsNew) return;
            var confirm = await Shell.Current.DisplayAlertAsync("Delete", "Delete this shopping list?", "Delete", "Cancel");
            if (!confirm) return;
            IsBusy = true; ClearMessages();
            try
            {
                Guid.TryParse(ShoppingListId, out var deleteId);
                var result = await shoppingListService.DeleteAsync(deleteId);
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

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task ExportAsync()
        {
            if (ShoppingListProducts.Count == 0) return;
            var text = string.Join(Environment.NewLine,
                ShoppingListProducts.Select(p => $"{p.Product?.Name} - {p.Quantity} {p.Unit?.Name}"));
            await Clipboard.SetTextAsync(text);
            await Shell.Current.DisplayAlertAsync("Exported", "Shopping list copied to clipboard.", "OK");
        }

        [RelayCommand]
        private void RemoveProduct(ShoppingListProductEditModel product) =>
            ShoppingListProducts.Remove(product);
    }
}
