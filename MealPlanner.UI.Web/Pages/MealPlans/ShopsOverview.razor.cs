using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Constants;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Services.MealPlans;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.MealPlans
{
    [Authorize]
    public partial class ShopsOverview
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = default!;
        private GridTemplate<ShopModel>? _shopsGrid = default!;
        private string _tableGridClass = CssClasses.GridTemplateEmptyClass;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Inject]
        public IShopService? ShopService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public ISessionStorageService? SessionStorage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _navItems = new List<BreadcrumbItem>
            {
                 new BreadcrumbItem{ Text = "Home", Href ="recipebooks/recipesoverview" }
            };
            await RefreshAsync();
        }

        private void New()
        {
            NavigationManager?.NavigateTo($"mealplans/shopedit/");
        }

        private void Update(ShopModel item)
        {
            NavigationManager?.NavigateTo($"mealplans/shopedit/{item.Id}");
        }

        private async Task DeleteAsync(ShopModel item)
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

                var response = await ShopService!.DeleteAsync(item.Id);
                if (response != null && !response.Succeeded)
                {
                    MessageComponent?.ShowError(response.Message!);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    await _shopsGrid!.RefreshDataAsync();
                }
            }
        }

        private async Task RefreshAsync()
        {
            var request = new GridDataProviderRequest<ShopModel>();
            var queryParameters = await SessionStorage!.GetItemAsync<QueryParameters<ShopModel>>();
            if (queryParameters != null)
            {
                request = new GridDataProviderRequest<ShopModel>
                {
                    Filters = queryParameters.Filters != null ? queryParameters.Filters : new List<FilterItem>(),
                    Sorting = queryParameters.Sorting != null ? queryParameters.Sorting.Select(QueryParameters<ShopModel>.FromModel).ToList() : new List<SortingItem<ShopModel>>(),
                    PageNumber = queryParameters.PageNumber,
                    PageSize = queryParameters.PageSize,
                };
            }
            else
            {
                request = new GridDataProviderRequest<ShopModel>
                {
                    Filters = new List<FilterItem>() { },
                    Sorting = new List<SortingItem<ShopModel>>
                        {
                            new SortingItem<ShopModel>("Name", item => item.Name!, SortDirection.Ascending),
                        },
                    PageNumber = 1,
                    PageSize = 10
                };
            }
            await DataProviderAsync(request);
        }

        private async Task<GridDataProviderResult<ShopModel>> DataProviderAsync(GridDataProviderRequest<ShopModel> request)
        {
            var queryParameters = new QueryParameters<ShopModel>()
            {
                Filters = request.Filters,
                Sorting = request.Sorting?.Select(x => QueryParameters<ShopModel>.ToModel(x)).ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };

            var result = await ShopService!.SearchAsync(queryParameters);
            if (result == null || result.Items == null)
            {
                result = new PagedList<ShopModel>(new List<ShopModel>(), new Metadata());
            }
            await SessionStorage!.SetItemAsync(queryParameters);
            _tableGridClass = result!.Items!.Count == 0 ? CssClasses.GridTemplateEmptyClass : CssClasses.GridTemplateWithItemsClass;
            StateHasChanged();
            return new GridDataProviderResult<ShopModel> { Data = result!.Items, TotalCount = result.Metadata!.TotalCount };
        }
    }
}
