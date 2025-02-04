using BlazorBootstrap;
using Common.Pagination;
using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ProductsOverview
    {
        private List<BreadcrumbItem>? NavItems { get; set; }

        [Inject]
        public IProductService? ProductService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [CascadingParameter(Name = "MessageComponent")]
        protected IMessageComponent? MessageComponent { get; set; }

        protected ConfirmDialog dialog = default!;
        protected GridTemplate<ProductModel>? productsGrid;

        protected override async Task OnInitializedAsync()
        {
            NavItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem{ Text = "Home", Href ="/recipesoverview" }
            };
            await RefreshAsync();
        }

        private void New()
        {
            NavigationManager?.NavigateTo($"productedit/");
        }

        private void Update(ProductModel item)
        {
            NavigationManager?.NavigateTo($"productedit/{item.Id}");
        }

        private async Task DeleteAsync(ProductModel item)
        {
            if (item != null)
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

                var response = await ProductService!.DeleteAsync(item.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    MessageComponent?.ShowError(response);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    await productsGrid!.RefreshDataAsync();
                }
            }
        }

        private async Task RefreshAsync()
        {
            var request = new GridDataProviderRequest<ProductModel>
            {
                Filters = new List<FilterItem>() { },
                Sorting = new List<SortingItem<ProductModel>>
                        {
                            new SortingItem<ProductModel>("Name", item => item.Name!, SortDirection.Ascending),
                        },
                PageNumber = 1,
                PageSize = 10
            };
            await ProductsDataProviderAsync(request);
        }

        private async Task<GridDataProviderResult<ProductModel>> ProductsDataProviderAsync(GridDataProviderRequest<ProductModel> request)
        {
            string sortString = "";
            SortDirection sortDirection = SortDirection.None;

            if (request.Sorting is not null && request.Sorting.Any())
            {
                sortString = request.Sorting.FirstOrDefault()!.SortString;
                sortDirection = request.Sorting.FirstOrDefault()!.SortDirection;
            }
            var queryParameters = new QueryParameters()
            {
                Filters = request.Filters,
                SortString = sortString,
                SortDirection = sortDirection,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };

            var result = await ProductService!.SearchAsync(queryParameters);
            return await Task.FromResult(new GridDataProviderResult<ProductModel> { Data = result!.Items, TotalCount = result.Metadata!.TotalCount });
        }
    }
}
