using System.ComponentModel.DataAnnotations;
using BlazorBootstrap;
using Blazored.Modal.Services;
using Common.Models;
using Common.Pagination;
using Common.UI;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Pages.RecipeBooks;
using MealPlanner.UI.Web.Services.MealPlans;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RecipeBook.Shared.Converters;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages.MealPlans
{
    [Authorize]
    public partial class ShoppingListEdit
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = default!;
        private ShopEditModel? _shop;

        [CascadingParameter]
        private IModalService? ModalService { get; set; }

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Parameter]
        public string? Id { get; set; }

        public ShoppingListEditModel? ShoppingList { get; set; }

        public PagedList<ProductCategoryModel>? Categories { get; set; }

        public string? ProductId { get; set; }
        public PagedList<ProductModel>? Products { get; set; }

        public PagedList<ShopModel>? Shops { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "The quantity for the product must be a positive number.")]
        public string? Quantity { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a unit of measurement for the ingredient.")]
        public string? UnitId { get; set; }

        public IList<UnitModel>? Units { get; set; }
        public PagedList<UnitModel>? BaseUnits { get; set; }

        [Inject]
        public IShoppingListService ShoppingListService { get; set; } = default!;

        [Inject]
        public IProductCategoryService ProductCategoryService { get; set; } = default!;

        [Inject]
        public IProductService ProductService { get; set; } = default!;

        [Inject]
        public IShopService ShopService { get; set; } = default!;

        [Inject]
        public IMealPlanService MealPlanService { get; set; } = default!;

        [Inject]
        public IRecipeService RecipeService { get; set; } = default!;

        [Inject]
        public IUnitService UnitService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public IJSRuntime JS { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            _navItems =
            [
                new() { Text = "Shopping lists", Href = "mealplans/shoppinglistsoverview" },
                new() { Text = "Shopping list", IsCurrentPage = true },
            ];

            var queryParametersProduct = new QueryParameters<ProductCategoryModel>
            {
                Filters = [],
                Sorting =
                [
                    new SortingModel
                    {
                        PropertyName = "Name",
                        Direction = SortDirection.Ascending
                    }
                ],
                PageSize = int.MaxValue,
                PageNumber = 1
            };

            Categories = await ProductCategoryService.SearchAsync(queryParametersProduct);
            Shops = await ShopService.SearchAsync();
            BaseUnits = await UnitService.SearchAsync();

            _ = int.TryParse(Id, out var id);
            if (id == 0)
            {
                ShoppingList = new ShoppingListEditModel
                {
                    Products = []
                };
            }
            else
            {
                ShoppingList = await ShoppingListService.GetEditAsync(id);
                if (ShoppingList is not null)
                {
                    await OnShopChangedAsync(new ChangeEventArgs { Value = ShoppingList.ShopId });
                }
            }
        }

        private async Task SaveAsync()
        {
            if (ShoppingList is null)
                return;

            await SaveCoreAsync(ShoppingList);
        }

        private async Task SaveCoreAsync(ShoppingListEditModel shoppingList)
        {
            CommandResponse? response;

            if (shoppingList.Id == 0)
            {
                response = await ShoppingListService.AddAsync(shoppingList);
            }
            else
            {
                response = await ShoppingListService.UpdateAsync(shoppingList);
            }

            if (response is null)
            {
                await ShowErrorAsync("Save failed. Please try again.");
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? "Save failed.");
                return;
            }

            await ShowInfoAsync("Data has been saved successfully");
            NavigateToOverview();
        }

        private async Task DeleteAsync()
        {
            if (ShoppingList is null || ShoppingList.Id == 0)
                return;

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

            await DeleteCoreAsync(ShoppingList);
        }

        private async Task DeleteCoreAsync(ShoppingListEditModel shoppingList)
        {
            if (shoppingList.Id == 0)
                return;

            var response = await ShoppingListService.DeleteAsync(shoppingList.Id);
            if (response is null)
            {
                await ShowErrorAsync("Delete failed. Please try again.");
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? "Delete failed.");
                return;
            }

            await ShowInfoAsync("Data has been deleted successfully");
            NavigateToOverview();
        }

        private async Task DeleteProductAsync(ProductModel item)
        {
            var itemToDelete = ShoppingList?.Products?.FirstOrDefault(i => i.Product?.Id == item.Id);
            if (itemToDelete is null)
                return;

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

        private bool CanAddProduct =>
            !string.IsNullOrWhiteSpace(ProductId) &&
            ProductId != "0" &&
            !string.IsNullOrWhiteSpace(Quantity) &&
            double.TryParse(Quantity, out _);

        private async Task AddProductAsync()
        {
            if (!string.IsNullOrWhiteSpace(ProductId) &&
                ProductId != "0" &&
                UnitId != "0")
            {
                var product = Products?.Items?.FirstOrDefault(i => i.Id == int.Parse(ProductId));
                if (product is null)
                    return;

                await AddProductAsync(product, decimal.Parse(Quantity!), int.Parse(UnitId!));
            }
        }

        private bool CanAddMealPlan => ShoppingList?.ShopId != 0;

        private async Task AddMealPlanAsync()
        {
            if (!CanAddMealPlan)
            {
                await ShowErrorAsync("You must select a shop first.");
                return;
            }

            var modal = ModalService?.Show<MealPlanSelection>("Select a meal plan");
            if (modal is null)
                return;

            var result = await modal.Result;

            if (!result.Confirmed || result.Data is null)
            {
                await ShowErrorAsync("You must select a meal plan to add to the shopping list.");
                return;
            }

            if (!int.TryParse(result.Data.ToString(), out var mealPlanId))
            {
                await ShowErrorAsync("You must select a meal plan to add to the shopping list.");
                return;
            }

            var products = await MealPlanService.GetShoppingListProductsAsync(mealPlanId, ShoppingList!.ShopId);
            if (products is null)
                return;

            foreach (var item in products)
            {
                if (item.Product is not null)
                {
                    await AddProductAsync(item.Product, item.Quantity, item.UnitId);
                }
            }
        }

        private bool CanAddRecipe => ShoppingList?.ShopId != 0;

        private async Task AddRecipeAsync()
        {
            if (!CanAddRecipe)
            {
                await ShowErrorAsync("You must select a shop first.");
                return;
            }

            var modal = ModalService?.Show<RecipeSelection>("Select a recipe");
            if (modal is null)
                return;

            var result = await modal.Result;

            if (!result.Confirmed || result.Data is null)
            {
                await ShowErrorAsync("You must select a recipe to add to the shopping list.");
                return;
            }

            var recipeIdString = result.Data.ToString();
            if (!int.TryParse(recipeIdString, out var recipeId))
            {
                await ShowErrorAsync("You must select a recipe to add to the shopping list.");
                return;
            }

            var products = await RecipeService.GetShoppingListProductsAsync(recipeId, ShoppingList!.ShopId);
            if (products is null)
                return;

            foreach (var item in products)
            {
                if (item.Product is not null)
                {
                    await AddProductAsync(item.Product, item.Quantity, item.UnitId);
                }
            }
        }

        private async Task AddProductAsync(ProductModel product, decimal quantity, int unitId)
        {
            if (ShoppingList?.Products is null || BaseUnits?.Items is null)
                return;

            var item = ShoppingList.Products.FirstOrDefault(i => i.Product?.Id == product.Id);
            var unit = BaseUnits.Items.FirstOrDefault(i => i.Id == unitId);
            var baseUnit = product.BaseUnit;

            if (unit is null || baseUnit is null)
            {
                await ShowErrorAsync("Unit configuration is invalid.");
                return;
            }

            try
            {
                if (item != null)
                {
                    item.Quantity += UnitConverter.Convert(quantity, unit, baseUnit);
                }
                else
                {
                    var displaySequence = _shop?.GetDisplaySequence(product.ProductCategory?.Id);

                    item = new ShoppingListProductEditModel
                    {
                        ShoppingListId = ShoppingList.Id,
                        Collected = false,
                        Product = product,
                        Quantity = UnitConverter.Convert(quantity, unit, baseUnit),
                        UnitId = baseUnit.Id,
                        Unit = baseUnit,
                        DisplaySequence = displaySequence != null ? displaySequence.Value : 1
                    };

                    ShoppingList.Products.Add(item);
                    ShoppingList.Products.SetIndexes();
                    Quantity = string.Empty;
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }

            StateHasChanged();
        }

        private void NavigateToOverview()
        {
            NavigationManager.NavigateTo("mealplans/shoppinglistsoverview");
        }

        private async Task CheckboxChangedAsync(ShoppingListProductEditModel model)
        {
            var itemToChange = ShoppingList?.Products?.FirstOrDefault(item => item.Product?.Id == model?.Product?.Id);

            if (itemToChange is not null)
            {
                itemToChange.Collected = !itemToChange.Collected;
            }

            if (ShoppingList is null)
                return;

            var response = await ShoppingListService.UpdateAsync(ShoppingList);
            if (response != null && !response.Succeeded)
            {
                await ShowErrorAsync(response.Message!);
            }
            else
            {
                await OnInitializedAsync();
                StateHasChanged();
            }
        }

        private async Task OnProductCategoryChangedAsync(ChangeEventArgs e)
        {
            var productCategoryId = e.Value?.ToString();
            var filters = new List<FilterItem>();

            if (!string.IsNullOrWhiteSpace(productCategoryId))
            {
                filters.Add(new FilterItem(
                    "ProductCategoryId",
                    productCategoryId,
                    FilterOperator.Equals,
                    StringComparison.OrdinalIgnoreCase));
            }

            var queryParameters = new QueryParameters<ProductModel>
            {
                Filters = filters,
                Sorting =
                [
                    new SortingModel
                    {
                        PropertyName = "Name",
                        Direction = SortDirection.Ascending
                    }
                ],
                PageSize = int.MaxValue,
                PageNumber = 1
            };

            Products = await ProductService.SearchAsync(queryParameters);

            ProductId = string.Empty;
            Quantity = string.Empty;

            StateHasChanged();
        }

        private async Task OnProductChangedAsync(ChangeEventArgs e)
        {
            var productId = e.Value?.ToString();
            ProductId = productId;
            Quantity = string.Empty;

            if (string.IsNullOrWhiteSpace(productId))
            {
                StateHasChanged();
                return;
            }

            var product = await ProductService.GetEditAsync(int.Parse(productId));
            if (product == null || BaseUnits?.Items == null)
            {
                StateHasChanged();
                return;
            }

            var baseUnit = BaseUnits.Items.FirstOrDefault(x => x.Id == product.BaseUnitId);
            if (baseUnit == null)
            {
                StateHasChanged();
                return;
            }

            Units = BaseUnits.Items.Where(x => x.UnitType == baseUnit.UnitType).ToList();

            StateHasChanged();
        }

        private async Task OnShopChangedAsync(ChangeEventArgs e)
        {
            if (!int.TryParse(e.Value?.ToString(), out var shopId))
                return;

            if (ShoppingList is null)
                return;

            ShoppingList.ShopId = shopId;
            _shop = await ShopService.GetEditAsync(ShoppingList.ShopId);

            if (ShoppingList.Products is not { Count: > 0 } || _shop is null)
                return;

            foreach (var item in ShoppingList.Products)
            {
                var displaySequence = _shop.GetDisplaySequence(item.Product?.ProductCategory?.Id);
                item.DisplaySequence = displaySequence != null ? displaySequence.Value : 1;
            }

            ShoppingList.Products = ShoppingList.Products
                .OrderBy(item => item.Collected)
                .ThenBy(item => item.DisplaySequence)
                .ThenBy(item => item.Product?.Name)
                .ToList();

            StateHasChanged();
        }

        private async Task CheckQuantityAsync(ChangeEventArgs _)
        {
            await JS.InvokeVoidAsync("checkQuantity");
        }

        private async Task ShowErrorAsync(string message)
            => await MessageComponent!.ShowErrorAsync(message);

        private async Task ShowInfoAsync(string message)
            => await MessageComponent!.ShowInfoAsync(message);
    }
}