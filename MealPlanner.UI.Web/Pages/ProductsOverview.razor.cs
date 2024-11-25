using System.Text.Json;
using BlazorBootstrap;
using Common.Pagination;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ProductsOverview
    {
        private List<BreadcrumbItem>? NavItems { get; set; }

        public ProductModel? Product { get; set; }
        
        public string? CategoryId { get; set; }
        public IList<ProductCategoryModel>? Categories { get; set; }

        [Inject]
        public IProductService? ProductService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IJSRuntime JS { get; set; } = default!;

        [CascadingParameter(Name = "MessageComponent")]
        protected IMessageComponent? MessageComponent { get; set; }

        protected ConfirmDialog dialog = default!;

        protected override async Task OnInitializedAsync()
        {
            NavItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem{ Text = "Home", Href ="/" },
                new BreadcrumbItem{ Text = "Products", IsCurrentPage = true }
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

        private async void DeleteAsync(ProductModel item)
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
                    NavigationManager?.NavigateTo("productsoverview", forceLoad: true);
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
            await ProductsDataProvider(request);
        }

        private async Task<GridDataProviderResult<ProductModel>> ProductsDataProvider(GridDataProviderRequest<ProductModel> request)
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

        private async Task OnGridSettingsChanged(GridSettings settings)
        {
            if (settings is null)
                return;

            await JS.InvokeVoidAsync("window.localStorage.setItem", "grid-settings", JsonSerializer.Serialize(settings));
        }

        private async Task<GridSettings> GridSettingsProvider()
        {
            var settingsJson = await JS.InvokeAsync<string>("window.localStorage.getItem", "grid-settings");
            if (string.IsNullOrWhiteSpace(settingsJson))
                return null!;

            var settings = JsonSerializer.Deserialize<GridSettings>(settingsJson);
            if (settings is null)
                return null!;

            return settings;
        }
    }
}
