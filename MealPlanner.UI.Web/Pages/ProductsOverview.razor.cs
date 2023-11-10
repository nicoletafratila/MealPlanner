using Common.Pagination;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ProductsOverview
    {
        public PagedList<ProductModel>? Products { get; set; }
        public ProductModel? Product { get; set; }
        public IList<ProductCategoryModel>? Categories { get; set; }
        public string? CategoryId { get; set; }

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

        [Parameter]
        public QueryParameters? QueryParameters { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            Categories = await CategoryService!.GetAllAsync();
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

                var response = await ProductService!.DeleteAsync(item.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    ErrorComponent!.ShowError("Error", response);
                }
                else
                {
                    await RefreshAsync();
                }
            }
        }

        protected async Task RefreshAsync()
        {
            Products = await ProductService!.SearchAsync(CategoryId, QueryParameters!);
            StateHasChanged();
        }

        private async void OnCategoryChangedAsync(ChangeEventArgs e)
        {
            CategoryId = e!.Value!.ToString();
            QueryParameters = new();
            await RefreshAsync();
        }

        private async Task OnPageChangedAsync(int pageNumber)
        {
            QueryParameters!.PageNumber = pageNumber;
            await RefreshAsync();
        }
    }
}
