using System.ComponentModel.DataAnnotations;
using BlazorBootstrap;
using Blazored.Modal.Services;
using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RecipeBook.Shared.Converters;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    [Authorize]
    public partial class ShoppingListEdit
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = default!;
        private ShopEditModel? _shop;

        [CascadingParameter]
        private IModalService? ModalService { get; set; } = default!;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Parameter]
        public string? Id { get; set; }
        public ShoppingListEditModel? ShoppingList { get; set; }

        private string? _productCategoryId;
        public string? ProductCategoryId
        {
            get
            {
                return _productCategoryId;
            }
            set
            {
                if (_productCategoryId != value)
                {
                    _productCategoryId = value;
                    OnProductCategoryChangedAsync(_productCategoryId!);
                }
            }
        }
        public PagedList<ProductCategoryModel>? ProductCategories { get; set; }

        private string? _productId;
        public string? ProductId
        {
            get
            {
                return _productId;
            }
            set
            {
                if (_productId != value)
                {
                    _productId = value;
                    Quantity = string.Empty;
                    UnitId = string.Empty;
                    OnProductChangedAsync(_productId!);
                }
            }
        }
        public PagedList<ProductModel>? Products { get; set; }

        private string? _shopId;
        public string? ShopId
        {
            get
            {
                return _shopId;
            }
            set
            {
                if (_shopId != value)
                {
                    _shopId = value;
                    OnShopChangedAsync(value!);
                }
            }
        }
        public PagedList<ShopModel>? Shops { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "The quantity for the product must be a positive number.")]
        public string? Quantity { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a unit of measurement for the ingredient.")]
        public string? UnitId { get; set; }
        public IList<UnitModel>? Units { get; set; }
        public PagedList<UnitModel>? BaseUnits { get; set; }

        [Inject]
        public IShoppingListService? ShoppingListService { get; set; }

        [Inject]
        public IProductCategoryService? ProductCategoryService { get; set; }

        [Inject]
        public IProductService? ProductService { get; set; }

        [Inject]
        public IShopService? ShopService { get; set; }

        [Inject]
        public IMealPlanService? MealPlanService { get; set; }

        [Inject]
        public IRecipeService? RecipeService { get; set; }

        [Inject]
        public IUnitService? UnitService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IJSRuntime JS { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            _navItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem{ Text = "Shopping lists", Href ="/shoppinglistsoverview" },
                new BreadcrumbItem{ Text = "Shopping list", IsCurrentPage = true },
            };

            Shops = await ShopService!.SearchAsync();
            ProductCategories = await ProductCategoryService!.SearchAsync();
            BaseUnits = await UnitService!.SearchAsync();

            _ = int.TryParse(Id, out int id);
            if (id == 0)
            {
                ShoppingList = new ShoppingListEditModel();
                ShoppingList.Products = new List<ShoppingListProductEditModel>();
            }
            else
            {
                ShoppingList = await ShoppingListService!.GetEditAsync(id);
                ShopId = ShoppingList!.ShopId.ToString();
            }
        }

        private async Task SaveAsync()
        {
            var response = ShoppingList?.Id == 0 ? await ShoppingListService!.AddAsync(ShoppingList) : await ShoppingListService!.UpdateAsync(ShoppingList!);
            if (response != null && !response.Succeeded)
            {
                MessageComponent?.ShowError(response.Message!);
            }
            else
            {
                MessageComponent?.ShowInfo("Data has been saved successfully");
                NavigateToOverview();
            }
        }

        private async Task DeleteAsync()
        {
            if (ShoppingList?.Id != 0)
            {
                var options = new ConfirmDialogOptions
                {
                    YesButtonText = "OK",
                    YesButtonColor = ButtonColor.Success,
                    NoButtonText = "Cancel",
                    NoButtonColor = ButtonColor.Danger
                };
                var confirmation = await _dialog.ShowAsync(
                        title: "Are you sure you want to delete this?",
                        message1: "This will delete the record. Once deleted can not be rolled back.",
                        message2: "Do you want to proceed?",
                        confirmDialogOptions: options);

                if (!confirmation)
                    return;

                var response = await ShoppingListService!.DeleteAsync(ShoppingList!.Id);
                if (response != null && !response.Succeeded)
                {
                    MessageComponent?.ShowError(response.Message!);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    NavigateToOverview();
                }
            }
        }

        private async Task DeleteProductAsync(ProductModel item)
        {
            ShoppingListProductEditModel? itemToDelete = ShoppingList?.Products?.FirstOrDefault(i => i.Product?.Id == item.Id);
            if (itemToDelete != null)
            {
                var options = new ConfirmDialogOptions
                {
                    YesButtonText = "OK",
                    YesButtonColor = ButtonColor.Success,
                    NoButtonText = "Cancel",
                    NoButtonColor = ButtonColor.Danger
                };
                var confirmation = await _dialog.ShowAsync(
                        title: "Are you sure you want to delete this?",
                        message1: "This will delete the record. Once deleted can not be rolled back.",
                        message2: "Do you want to proceed?",
                        confirmDialogOptions: options);

                if (!confirmation)
                    return;

                ShoppingList?.Products?.Remove(itemToDelete);
                ShoppingList?.Products?.SetIndexes();
                StateHasChanged();
            }
        }

        private bool CanAddProduct
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ProductId) &&
                       ProductId != "0" &&
                       !string.IsNullOrWhiteSpace(Quantity) &&
                       double.TryParse(Quantity, out _);
            }
        }

        private void AddProduct()
        {
            if (!string.IsNullOrWhiteSpace(ProductId) && ProductId != "0" && UnitId != "0")
            {
                AddProduct(Products?.Items?.FirstOrDefault(i => i.Id == int.Parse(ProductId))!, decimal.Parse(Quantity!), int.Parse(UnitId!));
            }
        }

        private bool CanAddMealPlan
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ShopId) &&
                       ShopId != "0";
            }
        }

        private async Task AddMealPlanAsync()
        {
            var mealPlanSelectionModal = ModalService?.Show<MealPlanSelection>("Select a meal plan");
            var result = await mealPlanSelectionModal!.Result;

            if (result.Confirmed && result?.Data != null)
            {
                int mealPlanId;
                if (!int.TryParse(result.Data.ToString(), out mealPlanId))
                {
                    MessageComponent?.ShowError("You must select a meal plan to add to the shopping list.");
                    return;
                }
                var products = await MealPlanService!.GetShoppingListProductsAsync(mealPlanId, ShoppingList!.ShopId);
                foreach (var item in products!)
                {
                    AddProduct(item.Product!, item.Quantity, item.UnitId);
                }
            }
        }

        private bool CanAddRecipe
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ShopId) &&
                       ShopId != "0";
            }
        }

        private async Task AddRecipeAsync()
        {
            var recipeSelectionModal = ModalService?.Show<RecipeSelection>("Select a recipe");
            var result = await recipeSelectionModal!.Result;

            if (result.Confirmed && result?.Data != null)
            {
                int recipeId;
                if (!int.TryParse(result.Data.ToString(), out recipeId))
                {
                    MessageComponent?.ShowError("You must select a recipe to add to the shopping list.");
                    return;
                }
                var products = await RecipeService!.GetShoppingListProductsAsync(recipeId, ShoppingList!.ShopId);
                foreach (var item in products!)
                {
                    AddProduct(item.Product!, item.Quantity, item.UnitId);
                }
            }
        }

        private void AddProduct(ProductModel product, decimal quantity, int unitId)
        {
            ShoppingListProductEditModel? item = ShoppingList!.Products!.FirstOrDefault(i => i.Product?.Id == product.Id);
            UnitModel? unit = BaseUnits!.Items!.FirstOrDefault(i => i.Id == unitId);

            try
            {
                if (item != null)
                {
                    item.Quantity += UnitConverter.Convert(quantity, unit!, product!.BaseUnit!);
                }
                else
                {
                    item = new ShoppingListProductEditModel
                    {
                        ShoppingListId = ShoppingList.Id,
                        Collected = false,
                        Product = product,
                        Quantity = UnitConverter.Convert(quantity, unit!, product!.BaseUnit!),
                        UnitId = product!.BaseUnit!.Id,
                        Unit = product!.BaseUnit!,
                        DisplaySequence = _shop!.DisplaySequence!.FirstOrDefault(i => i.ProductCategory?.Id == product.ProductCategory?.Id)!.Value
                    };
                    ShoppingList.Products?.Add(item);
                    ShoppingList.Products?.SetIndexes();
                    Quantity = string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageComponent?.ShowError(ex.Message);
            }

            StateHasChanged();
        }

        private void NavigateToOverview()
        {
            NavigationManager?.NavigateTo($"/shoppinglistsoverview");
        }

        private async Task CheckboxChangedAsync(ShoppingListProductEditModel model)
        {
            var itemToChange = ShoppingList?.Products?.FirstOrDefault(item => item.Product?.Id == model?.Product?.Id);
            if (itemToChange != null)
            {
                itemToChange.Collected = !itemToChange.Collected;
            }

            var response = await ShoppingListService!.UpdateAsync(ShoppingList!);
            if (response != null && !response.Succeeded)
            {
                MessageComponent?.ShowError(response.Message!);
            }
            else
            {
                await OnInitializedAsync();
                StateHasChanged();
            }
        }

        private async Task OnProductCategoryChangedAsync(string value)
        {
            var filters = new List<FilterItem>();
            if (!string.IsNullOrWhiteSpace(value))
            {
                filters.Add(new FilterItem(nameof(ProductCategoryId), value, FilterOperator.Equals, StringComparison.OrdinalIgnoreCase));
            };
            var queryParameters = new QueryParameters<ProductModel>()
            {
                Filters = filters,
                Sorting = new List<SortingModel>() { new SortingModel() { PropertyName = "Name", Direction = SortDirection.Ascending } },
                PageSize = int.MaxValue,
                PageNumber = 1
            };
            Products = await ProductService!.SearchAsync(queryParameters);

            ProductCategoryId = value;
            ProductId = string.Empty;
            Quantity = string.Empty;
            StateHasChanged();
        }

        private async Task OnProductChangedAsync(string value)
        {
            Quantity = string.Empty;
            if (!string.IsNullOrWhiteSpace(value))
            {
                var product = await ProductService!.GetEditAsync(int.Parse(value));
                if (product != null)
                {
                    var baseUnit = BaseUnits!.Items!.FirstOrDefault(x => x.Id == product.BaseUnitId);
                    Units = BaseUnits!.Items!.Where(x => x.UnitType == baseUnit!.UnitType).ToList();
                }
            }
            StateHasChanged();
        }

        private async Task OnShopChangedAsync(string value)
        {
            if (value == "0")
            {
                _shop = null;
                return;
            }

            ShoppingList!.ShopId = int.Parse(value);
            _shop = await ShopService!.GetEditAsync(ShoppingList!.ShopId);

            if (ShoppingList.Products != null && ShoppingList.Products.Any())
            {
                foreach (var item in ShoppingList.Products)
                {
                    var displaySequence = _shop?.GetDisplaySequence(item.Product?.ProductCategory?.Id)!;
                    item.DisplaySequence = displaySequence != null ? displaySequence.Value : 1;
                }
                ShoppingList.Products = ShoppingList.Products.OrderBy(item => item.Collected)
                                                             .ThenBy(item => item.DisplaySequence)
                                                             .ThenBy(item => item.Product?.Name).ToList();
                StateHasChanged();
            }
        }

        private async Task CheckQuantityAsync(ChangeEventArgs e)
        {
            await JS.InvokeVoidAsync("checkQuantity");
        }
    }
}
