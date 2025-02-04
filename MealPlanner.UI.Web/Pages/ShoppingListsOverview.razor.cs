using BlazorBootstrap;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ShoppingListsOverview
    {
        private List<BreadcrumbItem>? NavItems { get; set; }

        [Inject]
        public IShoppingListService? ShoppingListService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [CascadingParameter(Name = "MessageComponent")]
        protected IMessageComponent? MessageComponent { get; set; }

        protected ConfirmDialog dialog = default!;
        protected GridTemplate<ShoppingListModel>? shoppingListsGrid;

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
            NavigationManager?.NavigateTo($"shoppinglistedit/");
        }

        private void Update(ShoppingListModel item)
        {
            NavigationManager?.NavigateTo($"shoppinglistedit/{item.Id}");
        }

        private async Task DeleteAsync(ShoppingListModel item)
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

                var response = await ShoppingListService!.DeleteAsync(item.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    MessageComponent?.ShowError(response);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    await shoppingListsGrid!.RefreshDataAsync();
                }
            }
        }

        private async Task RefreshAsync()
        {
            var request = new GridDataProviderRequest<ShoppingListModel>
            {
                Filters = new List<FilterItem>() { },
                Sorting = new List<SortingItem<ShoppingListModel>>
                        {
                            new SortingItem<ShoppingListModel>("Name", item => item.Name!, SortDirection.Ascending),
                        },
                PageNumber = 1,
                PageSize = 10
            };
            await ShoppingListsDataProviderAsync(request);
        }

        private async Task<GridDataProviderResult<ShoppingListModel>> ShoppingListsDataProviderAsync(GridDataProviderRequest<ShoppingListModel> request)
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

            var result = await ShoppingListService!.SearchAsync(queryParameters);
            return await Task.FromResult(new GridDataProviderResult<ShoppingListModel> { Data = result!.Items, TotalCount = result.Metadata!.TotalCount });
        }
    }
}
