using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Constants;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    [Authorize]
    public partial class ShoppingListsOverview
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = default!;
        private GridTemplate<ShoppingListModel>? _shoppingListsGrid = default!;
        private string _tableGridClass = CssClasses.GridTemplateEmptyClass;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Inject]
        public IShoppingListService? ShoppingListService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public ISessionStorageService? SessionStorage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _navItems = new List<BreadcrumbItem>
            {
                 new BreadcrumbItem{ Text = "Home", Href ="recipesoverview" }
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
                var confirmation = await _dialog.ShowAsync(
                        title: "Are you sure you want to delete this?",
                        message1: "This will delete the record. Once deleted can not be rolled back.",
                        message2: "Do you want to proceed?",
                        confirmDialogOptions: options);

                if (!confirmation)
                    return;

                var response = await ShoppingListService!.DeleteAsync(item.Id);
                if (response != null && !response.Succeeded)
                {
                    MessageComponent?.ShowError(response.Message!);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    await _shoppingListsGrid!.RefreshDataAsync();
                }
            }
        }

        private async Task RefreshAsync()
        {
            var request = new GridDataProviderRequest<ShoppingListModel>();
            var queryParameters = await SessionStorage!.GetItemAsync<QueryParameters<ShoppingListModel>>();
            if (queryParameters != null)
            {
                request = new GridDataProviderRequest<ShoppingListModel>
                {
                    Filters = queryParameters.Filters != null ? queryParameters.Filters : new List<FilterItem>(),
                    Sorting = queryParameters.Sorting != null ? queryParameters.Sorting.Select(QueryParameters<ShoppingListModel>.FromModel).ToList() : new List<SortingItem<ShoppingListModel>>(),
                    PageNumber = queryParameters.PageNumber,
                    PageSize = queryParameters.PageSize,
                };
            }
            else
            {
                request = new GridDataProviderRequest<ShoppingListModel>
                {
                    Filters = new List<FilterItem>() { },
                    Sorting = new List<SortingItem<ShoppingListModel>>
                        {
                            new SortingItem<ShoppingListModel>("Name", item => item.Name!, SortDirection.Ascending),
                        },
                    PageNumber = 1,
                    PageSize = 10
                };
            }
            await DataProviderAsync(request);
        }

        private async Task<GridDataProviderResult<ShoppingListModel>> DataProviderAsync(GridDataProviderRequest<ShoppingListModel> request)
        {
            var queryParameters = new QueryParameters<ShoppingListModel>()
            {
                Filters = request.Filters,
                Sorting = request.Sorting?.Select(x => QueryParameters<ShoppingListModel>.ToModel(x)).ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };

            var result = await ShoppingListService!.SearchAsync(queryParameters);
            if (result == null || result.Items == null)
            {
                result = new PagedList<ShoppingListModel>(new List<ShoppingListModel>(), new Metadata());
            }
            await SessionStorage!.SetItemAsync(queryParameters);
            _tableGridClass = result!.Items!.Count == 0 ? CssClasses.GridTemplateEmptyClass : CssClasses.GridTemplateWithItemsClass;
            StateHasChanged();
            return new GridDataProviderResult<ShoppingListModel> { Data = result!.Items, TotalCount = result.Metadata!.TotalCount };
        }
    }
}
