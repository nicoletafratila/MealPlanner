using System.ComponentModel.DataAnnotations;
using BlazorBootstrap;
using Blazored.Modal.Services;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ShoppingListEdit
    {
        private EditShopModel? _shop;

        [Parameter]
        public string? Id { get; set; }
        public EditShoppingListModel? ShoppingList { get; set; }

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
        public IList<ProductCategoryModel>? ProductCategories { get; set; }

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
        public IList<ShopModel>? Shops { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "The quantity for the product must be a positive number.")]
        public string? Quantity { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a unit of measurement for the ingredient.")]
        public int UnitId { get; set; }
        public IList<UnitModel>? Units { get; set; }

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

        [CascadingParameter]
        protected IModalService? Modal { get; set; } = default!;

        [CascadingParameter(Name = "MessageComponent")]
        protected IMessageComponent? MessageComponent { get; set; }

        protected ConfirmDialog dialog = default!;

        protected override async Task OnInitializedAsync()
        {
            _ = int.TryParse(Id, out int id);
            ProductCategories = await ProductCategoryService!.GetAllAsync();
            Shops = await ShopService!.GetAllAsync();
            Units = await UnitService!.GetAllAsync();

            if (id == 0)
            {
                ShoppingList = new EditShoppingListModel();
            }
            else
            {
                ShoppingList = await ShoppingListService!.GetEditAsync(id);
                ShopId = ShoppingList!.ShopId.ToString();
            }
        }

        private async void SaveAsync()
        {
            var response = ShoppingList?.Id == 0 ? await ShoppingListService!.AddAsync(ShoppingList) : await ShoppingListService!.UpdateAsync(ShoppingList!);
            if (!string.IsNullOrWhiteSpace(response))
            {
                MessageComponent?.ShowError(response);
            }
            else
            {
                MessageComponent?.ShowInfo("Data has been saved successfully");
                NavigateToOverview();
            }
        }

        private async void DeleteAsync()
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
                var confirmation = await dialog.ShowAsync(
                        title: "Are you sure you want to delete this?",
                        message1: "This will delete the record. Once deleted can not be rolled back.",
                        message2: "Do you want to proceed?",
                        confirmDialogOptions: options);

                if (!confirmation)
                    return;

                var result = await ShoppingListService!.DeleteAsync(ShoppingList!.Id);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    MessageComponent?.ShowError(result);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    NavigateToOverview();
                }
            }
        }

        private async void DeleteProductAsync(ProductModel item)
        {
            ShoppingListProductModel? itemToDelete = ShoppingList?.Products?.FirstOrDefault(i => i.Product?.Id == item.Id);
            if (itemToDelete != null)
            {
                var options = new ConfirmDialogOptions
                {
                    YesButtonText = "OK",
                    YesButtonColor = ButtonColor.Success,
                    NoButtonText = "Cancel",
                    NoButtonColor = ButtonColor.Danger
                };
                var confirmation = await dialog.ShowAsync(
                        title: "Are you sure you want to delete this?",
                        message1: "This will delete the record. Once deleted can not be rolled back.",
                        message2: "Do you want to proceed?",
                        confirmDialogOptions: options);

                if (!confirmation)
                    return;

                ShoppingList?.Products?.Remove(itemToDelete);
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
                       decimal.TryParse(Quantity, out _);
            }
        }

        private void AddProduct()
        {
            if (!string.IsNullOrWhiteSpace(ProductId) && ProductId != "0")
            {
                AddProduct(Products?.Items?.FirstOrDefault(i => i.Id == int.Parse(ProductId))!, decimal.Parse(Quantity!), UnitId);
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

        private async void AddMealPlanAsync()
        {
            var mealPlanSelectionModal = Modal?.Show<MealPlanSelection>();
            var result = await mealPlanSelectionModal!.Result;

            if (result.Confirmed && result?.Data != null)
            {
                int mealPlanId;
                if (!int.TryParse(result.Data.ToString(), out mealPlanId))
                {
                    MessageComponent?.ShowError("You must select a meal plan to add to the shopping list.");
                    return;
                }
                var products = await MealPlanService!.GetShoppingListProducts(mealPlanId, ShoppingList!.ShopId);
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

        private async void AddRecipeAsync()
        {
            var recipeSelectionModal = Modal?.Show<RecipeSelection>();
            var result = await recipeSelectionModal!.Result;

            if (result.Confirmed && result?.Data != null)
            {
                int recipeId;
                if (!int.TryParse(result.Data.ToString(), out recipeId))
                {
                    MessageComponent?.ShowError("You must select a recipe to add to the shopping list.");
                    return;
                }
                var products = await RecipeService!.GetShoppingListProducts(recipeId, ShoppingList!.ShopId);
                foreach (var item in products!)
                {
                    AddProduct(item.Product!, item.Quantity, item.UnitId);
                }
            }
        }

        private void AddProduct(ProductModel product, decimal quantity, int unitId)
        {
            ShoppingListProductModel? item = ShoppingList!.Products!.FirstOrDefault(i => i.Product?.Id == product.Id);
            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                item = new ShoppingListProductModel
                {
                    ShoppingListId = ShoppingList.Id,
                    Collected = false,
                    Product = product,
                    Quantity = quantity,
                    UnitId = unitId,
                    Unit = Units!.FirstOrDefault(i => i.Id == unitId),
                    DisplaySequence = _shop!.DisplaySequence!.FirstOrDefault(i => i.ProductCategory?.Id == product.ProductCategory?.Id)!.Value
                };
                ShoppingList.Products?.Add(item);
                Quantity = string.Empty;
            }

            StateHasChanged();
        }

        private void NavigateToOverview()
        {
            NavigationManager?.NavigateTo($"/shoppinglistsoverview");
        }

        private async void CheckboxChangedAsync(ShoppingListProductModel model)
        {
            var itemToChange = ShoppingList?.Products?.FirstOrDefault(item => item.Product?.Id == model?.Product?.Id);
            if (itemToChange != null)
            {
                itemToChange.Collected = !itemToChange.Collected;
            }
            var response = await ShoppingListService!.UpdateAsync(ShoppingList!);

            if (!string.IsNullOrWhiteSpace(response))
            {
                MessageComponent?.ShowError(response);
            }
            else
            {
                await OnInitializedAsync();
                StateHasChanged();
            }
        }

        private async void OnProductCategoryChangedAsync(string value)
        {
            ProductCategoryId = value;
            ProductId = string.Empty;
            Products = await ProductService!.SearchAsync(ProductCategoryId);
            StateHasChanged();
        }

        private async void OnShopChangedAsync(string value)
        {
            ShoppingList!.ShopId = int.Parse(value);
            _shop = await ShopService!.GetEditAsync(ShoppingList!.ShopId);
            foreach (var item in ShoppingList.Products!)
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
}
