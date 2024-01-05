using System.ComponentModel.DataAnnotations;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RecipeBook.Shared.Models;

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

        [Inject]
        public IShoppingListService? ShoppingListService { get; set; }

        [Inject]
        public IProductCategoryService? ProductCategoryService { get; set; }

        [Inject]
        public IProductService? ProductService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        [CascadingParameter(Name = "ErrorComponent")]
        protected IErrorComponent? ErrorComponent { get; set; }

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);
            ProductCategories = await ProductCategoryService!.GetAllAsync();

            if (id == 0)
            {
                ShoppingList = new EditShoppingListModel();
            }
            else
            {
                ShoppingList = await ShoppingListService!.GetEditAsync(int.Parse(Id!));
            }
        }

        private async Task SaveAsync()
        {
            var response = ShoppingList!.Id == 0 ? await ShoppingListService!.AddAsync(ShoppingList) : await ShoppingListService!.UpdateAsync(ShoppingList);
            if (!string.IsNullOrWhiteSpace(response))
            {
                ErrorComponent!.ShowError("Error", response);
            }
            else
            {
                NavigateToOverview();
            }
        }

        private async Task DeleteAsync()
        {
            if (ShoppingList!.Id != 0)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the shopping list: '{ShoppingList.Name}'?"))
                    return;

                var result = await ShoppingListService!.DeleteAsync(ShoppingList.Id);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    ErrorComponent!.ShowError("Error", result);
                }
                else
                {
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

        private void AddProduct()
        {
            if (!string.IsNullOrWhiteSpace(ProductId) && ProductId != "0")
            {
                if (ShoppingList != null)
                {
                    if (ShoppingList.Products == null)
                    {
                        ShoppingList.Products = new List<ShoppingListProductModel>();
                    }
                    ShoppingListProductModel? item = ShoppingList.Products.FirstOrDefault(i => i.Product!.Id == int.Parse(ProductId));
                    if (item != null)
                    {
                        item.Quantity += decimal.Parse(Quantity!);
                    }
                    else
                    {
                        item = new ShoppingListProductModel();
                        item.Product = Products!.Items!.FirstOrDefault(i => i.Id == int.Parse(ProductId));
                        item.Quantity = decimal.Parse(Quantity!);
                        ShoppingList.Products!.Add(item);
                        Quantity = string.Empty;
                    }
                }
            }
        }

        private async Task DeleteProductAsync(ProductModel item)
        {
            ShoppingListProductModel? itemToDelete = ShoppingList!.Products!.FirstOrDefault(i => i.Product!.Id == item.Id);
            if (itemToDelete != null)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the product '{item.Name}'?"))
                    return;

                ShoppingList.Products!.Remove(itemToDelete);
            }
        }

        private void NavigateToOverview()
        {
            NavigationManager!.NavigateTo($"/shoppinglistsoverview");
        }

        private async void CheckboxChanged(ShoppingListProductModel model)
        {
            var itemToChange = ShoppingList!.Products!.FirstOrDefault(item => item.Product!.Id == model!.Product!.Id);
            if (itemToChange != null)
            {
                itemToChange.Collected = !itemToChange.Collected;
            }
            var response = await ShoppingListService!.UpdateAsync(ShoppingList);

            if (!string.IsNullOrWhiteSpace(response))
            {
                ErrorComponent!.ShowError("Error", response);
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
