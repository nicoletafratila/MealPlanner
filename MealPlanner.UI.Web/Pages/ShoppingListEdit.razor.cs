using BlazorBootstrap;
using Blazored.Modal.Services;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ShoppingListEdit
    {
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

        [Range(0, int.MaxValue, ErrorMessage = "The quantity for the product must be a positive number.")]
        public string? Quantity { get; set; }

        private EditShopModel? _shop;

        [Inject]
        public IShoppingListService? ShoppingListService { get; set; }

        [Inject]
        public IProductCategoryService? ProductCategoryService { get; set; }

        [Inject]
        public IProductService? ProductService { get; set; }

        [Inject]
        public IShopService? ShopService { get; set; }

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

            if (id == 0)
            {
                ShoppingList = new EditShoppingListModel();
            }
            else
            {
                ShoppingList = await ShoppingListService!.GetEditAsync(id);
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

        private async void AddProductAsync()
        {
            if (!string.IsNullOrWhiteSpace(ProductId) && ProductId != "0")
            {
                if (ShoppingList != null)
                {
                    await SetShopAsync();

                    if (ShoppingList.Products == null)
                    {
                        ShoppingList.Products = new List<ShoppingListProductModel>();
                    }

                    ShoppingListProductModel? item = ShoppingList.Products.FirstOrDefault(i => i.Product?.Id == int.Parse(ProductId));
                    if (item != null)
                    {
                        item.Quantity += decimal.Parse(Quantity!);
                    }
                    else
                    {
                        var product = Products?.Items?.FirstOrDefault(i => i.Id == int.Parse(ProductId));
                        item = new ShoppingListProductModel
                        {
                            ShoppingListId = ShoppingList.Id,
                            Collected = false,
                            Product = product,
                            Quantity = decimal.Parse(Quantity!),
                            DisplaySequence = _shop!.DisplaySequence!.FirstOrDefault(i => i.ProductCategory?.Id == product?.ProductCategory?.Id)!.Value
                        };
                        ShoppingList.Products?.Add(item);
                        Quantity = string.Empty;
                    }

                    StateHasChanged();
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

                ShoppingList!.Products?.Remove(itemToDelete);
                StateHasChanged();
            }
        }

        private async Task SetShopAsync()
        {
            int shopId = ShoppingList!.ShopId;
            if (shopId == 0)
            {
                var shopSelectionModal = Modal?.Show<ShopSelection>();
                var result = await shopSelectionModal!.Result;

                if (result.Cancelled)
                {
                    MessageComponent?.ShowError("You must select a shop for the list.");
                    return;
                }

                if (result.Confirmed && result?.Data != null)
                {
                    if (!int.TryParse(result.Data.ToString(), out shopId))
                    {
                        MessageComponent?.ShowError("You must select a shop for the list.");
                        return;
                    }
                }
            }
            _shop ??= await ShopService!.GetEditAsync(shopId);
            ShoppingList!.ShopId = shopId;
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
    }
}
