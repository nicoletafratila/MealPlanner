using BlazorBootstrap;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ShopsOverview
    {
        private List<BreadcrumbItem>? NavItems { get; set; }

        [Inject]
        public IShopService? ShopService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [CascadingParameter(Name = "MessageComponent")]
        protected IMessageComponent? MessageComponent { get; set; }

        protected ConfirmDialog dialog = default!;

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
            NavigationManager?.NavigateTo($"shopedit/");
        }

        private void Update(ShopModel item)
        {
            NavigationManager?.NavigateTo($"shopedit/{item.Id}");
        }

        private async void DeleteAsync(ShopModel item)
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

                var response = await ShopService!.DeleteAsync(item.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    MessageComponent?.ShowError(response);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    await RefreshAsync();
                }
            }
        }

        private async Task RefreshAsync()
        {
            var request = new GridDataProviderRequest<ShopModel>
            {
                Filters = new List<FilterItem>() { },
                Sorting = new List<SortingItem<ShopModel>>
                        {
                            new SortingItem<ShopModel>("Name", item => item.Name!, SortDirection.Ascending),
                        },
                PageNumber = 1,
                PageSize = 10
            };
            await ShopsDataProvider(request);
        }

        private async Task<GridDataProviderResult<ShopModel>> ShopsDataProvider(GridDataProviderRequest<ShopModel> request)
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

            var result = await ShopService!.SearchAsync(queryParameters);
            return await Task.FromResult(new GridDataProviderResult<ShopModel> { Data = result!.Items, TotalCount = result.Metadata!.TotalCount });
        }
    }
}
