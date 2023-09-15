using Common.Api;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ProductsOverview
    {
        public IList<ProductModel>? Products { get; set; }
        public ProductModel? Product { get; set; }
        public IList<ProductCategoryModel>? Categories { get; set; }

        [Inject]
        public IProductService? ProductService { get; set; }

        [Inject]
        public IProductCategoryService? CategoryService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        [CascadingParameter(Name = "ErrorComponent")]
        protected IErrorComponent? ErrorComponent { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await RefreshAsync();
        }

        protected void New()
        {
            NavigationManager!.NavigateTo($"productedit/");
        }

        protected void Update(ProductModel item)
        {
            NavigationManager!.NavigateTo($"productedit/{item.Id}");
        }

        protected async Task DeleteAsync(ProductModel item)
        {
            if (item != null)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the product: '{item.Name}'?"))
                    return;

                var result = await ProductService!.DeleteAsync(item.Id);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    ErrorComponent!.ShowError("Error", result);
                }
                else
                {
                    await RefreshAsync();
                }
            }
        }

        protected async Task RefreshAsync()
        {
            Products = await ProductService!.GetAllAsync();
            Categories = await CategoryService!.GetAllAsync();
        }

        private async void OnCategoryChangedAsync(ChangeEventArgs e)
        {
            var categoryId = e!.Value!.ToString();
            if (!string.IsNullOrWhiteSpace(categoryId) && categoryId != "0")
                Products = await ProductService!.SearchAsync(int.Parse(categoryId));
            else
                Products = await ProductService!.GetAllAsync();
            StateHasChanged();
        }
    }
}
