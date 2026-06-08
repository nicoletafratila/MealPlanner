using System.ComponentModel.DataAnnotations;
using BlazorBootstrap;
using Blazored.Modal.Services;
using Common.Models;
using Common.Pagination;
using Common.Services.Converters;
using Common.UI;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Pages.RecipeBooks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages.MealPlans
{
    [Authorize]
    public partial class ShoppingListEdit
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = default!;
        private ShopEditModel? _shop;
        private bool showCopiedMessage;

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

        [Range(0, int.MaxValue, ErrorMessageResourceType = typeof(Resources.ShoppingListEdit), ErrorMessageResourceName = "QuantityPositiveNumber")]
        public string? Quantity { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ShoppingListEdit), ErrorMessageResourceName = "SelectUnitOfMeasurement")]
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
                new() { Text = Resources.ShoppingListEdit.BreadcrumbShoppingLists, Href = "mealplans/shoppinglistsoverview" },
                new() { Text = Resources.ShoppingListEdit.BreadcrumbShoppingList, IsCurrentPage = true },
            ];

            var queryParametersProduct = new QueryParameters<ProductCategoryModel>
            {
                Filters = [],
                Sorting =
                [
                    new SortingModel
                    {
                        PropertyName = "Name",
                        Direction = Common.Pagination.SortDirection.Ascending
                    }
                ],
                PageSize = int.MaxValue,
                PageNumber = 1
            };

            Categories = await ProductCategoryService.SearchAsync(queryParametersProduct);
            Shops = await ShopService.SearchAsync();
            BaseUnits = await UnitService.SearchAsync();

            _ = Guid.TryParse(Id, out var id);
            if (id == Guid.Empty)
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

            if (shoppingList.Id == Guid.Empty)
            {
                response = await ShoppingListService.AddAsync(shoppingList);
            }
            else
            {
                response = await ShoppingListService.UpdateAsync(shoppingList);
            }

            if (response is null)
            {
                await ShowErrorAsync(Resources.ShoppingListEdit.SaveFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.ShoppingListEdit.SaveFailed);
                return;
            }

            await ShowInfoAsync(Resources.ShoppingListEdit.SaveSucceeded);
            NavigateToOverview();
        }

        private async Task DeleteAsync()
        {
            if (ShoppingList is null || ShoppingList.Id == Guid.Empty)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = Resources.ShoppingListEdit.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.ShoppingListEdit.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.ShoppingListEdit.DeleteDialogTitle,
                message1: Resources.ShoppingListEdit.DeleteDialogMessage1,
                message2: Resources.ShoppingListEdit.DeleteDialogMessage2,
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            await DeleteCoreAsync(ShoppingList);
        }

        private async Task DeleteCoreAsync(ShoppingListEditModel shoppingList)
        {
            if (shoppingList.Id == Guid.Empty)
                return;

            var response = await ShoppingListService.DeleteAsync(shoppingList.Id);
            if (response is null)
            {
                await ShowErrorAsync(Resources.ShoppingListEdit.DeleteFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.ShoppingListEdit.DeleteFailed);
                return;
            }

            await ShowInfoAsync(Resources.ShoppingListEdit.DeleteSucceeded);
            NavigateToOverview();
        }

        private async Task DeleteProductAsync(ProductModel item)
        {
            var itemToDelete = ShoppingList?.Products?.FirstOrDefault(i => i.Product?.Id == item.Id);
            if (itemToDelete is null)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = Resources.ShoppingListEdit.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.ShoppingListEdit.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.ShoppingListEdit.DeleteDialogTitle,
                message1: Resources.ShoppingListEdit.DeleteDialogMessage1,
                message2: Resources.ShoppingListEdit.DeleteDialogMessage2,
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
                var product = Products?.Items?.FirstOrDefault(i => i.Id == Guid.Parse(ProductId));
                if (product is null)
                    return;

                await AddProductAsync(product, decimal.Parse(Quantity!), Guid.Parse(UnitId!));
            }
        }

        private bool CanAddMealPlan => ShoppingList?.ShopId != Guid.Empty;

        private async Task AddMealPlanAsync()
        {
            if (!CanAddMealPlan)
            {
                await ShowErrorAsync(Resources.ShoppingListEdit.MustSelectShopFirst);
                return;
            }

            var modal = ModalService?.Show<MealPlanSelection>(Resources.ShoppingListEdit.SelectMealPlanModalTitle);
            if (modal is null)
                return;

            var result = await modal.Result;

            if (!result.Confirmed || result.Data is null)
            {
                await ShowErrorAsync(Resources.ShoppingListEdit.MustSelectMealPlan);
                return;
            }

            if (!Guid.TryParse(result.Data.ToString(), out var mealPlanId))
            {
                await ShowErrorAsync(Resources.ShoppingListEdit.MustSelectMealPlan);
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

        private bool CanAddRecipe => ShoppingList?.ShopId != Guid.Empty;

        private async Task AddRecipeAsync()
        {
            if (!CanAddRecipe)
            {
                await ShowErrorAsync(Resources.ShoppingListEdit.MustSelectShopFirst);
                return;
            }

            var modal = ModalService?.Show<RecipeSelection>(Resources.ShoppingListEdit.SelectRecipeModalTitle);
            if (modal is null)
                return;

            var result = await modal.Result;

            if (!result.Confirmed || result.Data is null)
            {
                await ShowErrorAsync(Resources.ShoppingListEdit.MustSelectRecipe);
                return;
            }

            var recipeIdString = result.Data.ToString();
            if (!Guid.TryParse(recipeIdString, out var recipeId))
            {
                await ShowErrorAsync(Resources.ShoppingListEdit.MustSelectRecipe);
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

        private async Task AddProductAsync(ProductModel product, decimal quantity, Guid unitId)
        {
            if (ShoppingList?.Products is null || BaseUnits?.Items is null)
                return;

            var item = ShoppingList.Products.FirstOrDefault(i => i.Product?.Id == product.Id);
            var unit = BaseUnits.Items.FirstOrDefault(i => i.Id == unitId);
            var baseUnit = product.BaseUnit;

            if (unit is null || baseUnit is null)
            {
                await ShowErrorAsync(Resources.ShoppingListEdit.InvalidUnitConfiguration);
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

        private async Task Export()
        {
            var text = string.Empty;

            if (ShoppingList != null && ShoppingList.Products != null && ShoppingList.Products.Any())
            {
                text = string.Join(
                    Environment.NewLine,
                    ShoppingList.Products.Select(p => p.Product!.Name + " - " + p.Quantity + " " + p.Unit!.Name)
                );
            }

            await JS.InvokeVoidAsync("copyTextToClipboard", text);

            showCopiedMessage = true;
            StateHasChanged();

            await Task.Delay(2000);

            showCopiedMessage = false;
            StateHasChanged();
        }

        private async Task CheckboxChangedAsync(ShoppingListProductEditModel model, bool newValue)
        {
            var itemToChange = ShoppingList?.Products?.FirstOrDefault(item => item.Product?.Id == model?.Product?.Id);
            if (itemToChange is null || ShoppingList is null)
                return;

            var oldValue = itemToChange.Collected;
            itemToChange.Collected = newValue;

            var response = await ShoppingListService.UpdateAsync(ShoppingList);
            if (response is null || !response.Succeeded)
            {
                await ShowErrorAsync(response?.Message ?? Resources.ShoppingListEdit.SaveFailed);
                itemToChange.Collected = oldValue;
                StateHasChanged();
                return;
            }

            if (ShoppingList.Products is { Count: > 0 })
            {
                ShoppingList.Products = ShoppingList.Products
                    .OrderBy(item => item.Collected)
                    .ThenBy(item => item.DisplaySequence)
                    .ThenBy(item => item.Product?.Name)
                    .ToList();
            }

            StateHasChanged();
        }

        private async Task OnProductCategoryChangedAsync(ChangeEventArgs e)
        {
            var productCategoryId = e.Value?.ToString();
            var filters = new List<Common.Pagination.FilterItem>();

            if (!string.IsNullOrWhiteSpace(productCategoryId))
            {
                filters.Add(new Common.Pagination.FilterItem(
                    "ProductCategoryId",
                    productCategoryId,
                    Common.Pagination.FilterOperator.Equals,
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
                        Direction = Common.Pagination.SortDirection.Ascending
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

            var product = await ProductService.GetEditAsync(Guid.Parse(productId));
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
            if (!Guid.TryParse(e.Value?.ToString(), out var shopId))
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