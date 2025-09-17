using BlazorBootstrap;
using Common.Constants;
using Common.Pagination;
using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    [Authorize]
    public partial class ProductsOverview
    {
        private List<BreadcrumbItem>? navItems { get; set; }

        protected ConfirmDialog dialog = default!;
        protected GridTemplate<ProductModel>? productsGrid;
        protected string tableGridClass { get; set; } = CssClasses.GridTemplateWithItemsClass;

        [CascadingParameter(Name = "MessageComponent")]
        protected IMessageComponent? messageComponent { get; set; }

        [Inject]
        public IProductService? ProductService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            navItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem{ Text = "Home", Href ="/" }
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
                if (response != null && !response.Succeeded)
                {
                    messageComponent?.ShowError(response.Message!);
                }
                else
                {
                    messageComponent?.ShowInfo("Data has been deleted successfully");
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
            if (result == null || result.Items == null)
            {
                result = new PagedList<ProductModel>(new List<ProductModel>(), new Metadata());
            }
            tableGridClass = result!.Items!.Any() ? CssClasses.GridTemplateWithItemsClass : CssClasses.GridTemplateEmptyClass;
            return await Task.FromResult(new GridDataProviderResult<ProductModel> { Data = result!.Items, TotalCount = result.Metadata!.TotalCount });
        }
    }
}
