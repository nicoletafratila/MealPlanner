using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Constants;
using Common.Pagination;
using Common.UI;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services.Identities;
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
        private List<BreadcrumbItem> _navItems = [];
        private GridTemplate<ShopModel>? _shopsGrid;
        private string _tableGridClass = CssClasses.GridTemplateEmptyClass;
        private SortDirection _nameSortDirection = SortDirection.Ascending;
        private int _gridKey = 0;
        private bool _firstLoad = true;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Inject]
        public IShopService ShopService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public ISessionStorageService SessionStorage { get; set; } = default!;

        protected override void OnInitialized()
        {
            _navItems =
            [
                new BreadcrumbItem { Text = Resources.ShopsOverview.BreadcrumbHome, Href = "recipebooks/recipesoverview" }
            ];
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            var stored = await SessionStorage.GetItemAsync<QueryParameters<ShopModel>>();
            var nameSort = stored?.Sorting?.FirstOrDefault(s => s.PropertyName == "Name");
            var direction = nameSort?.Direction ?? SortDirection.Ascending;

            if (direction != _nameSortDirection)
            {
                _nameSortDirection = direction;
                _gridKey++;
                StateHasChanged();
            }
        }

        private async Task<GridSettings?> SettingsProviderAsync()
            => await SessionStorage.GetItemAsync<QueryParameters<ShopModel>>();

        private void New()
        {
            NavigationManager.NavigateTo("mealplans/shopedit/");
        }

        private void Update(ShopModel item)
        {
            if (item is null)
                return;

            NavigationManager.NavigateTo($"mealplans/shopedit/{item.Id}");
        }

        private async Task DeleteAsync(ShopModel item)
        {
            if (item is null)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = Resources.ShopsOverview.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.ShopsOverview.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.ShopsOverview.DeleteDialogTitle,
                message1: Resources.ShopsOverview.DeleteDialogMessage1,
                message2: Resources.ShopsOverview.DeleteDialogMessage2,
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            await DeleteCoreAsync(item);
        }

        private async Task DeleteCoreAsync(ShopModel item)
        {
            var response = await ShopService.DeleteAsync(item.Id);
            if (response is null)
            {
                await ShowErrorAsync(Resources.ShopsOverview.DeleteFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.ShopsOverview.DeleteFailed);
                return;
            }

            await ShowInfoAsync(Resources.ShopsOverview.DeleteSucceeded);

            if (_shopsGrid is not null)
                await _shopsGrid.RefreshDataAsync();
        }

        private async Task<GridDataProviderResult<ShopModel>> DataProviderAsync(
            GridDataProviderRequest<ShopModel> request)
        {
            var queryParameters = new QueryParameters<ShopModel>
            {
                Filters = request.Filters,
                Sorting = request.Sorting?
                    .Select(QueryParameters<ShopModel>.ToModel)
                    .ToList()!,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };

            var isFirstLoad = _firstLoad;
            _firstLoad = false;

            var result = await ShopService.SearchAsync(queryParameters)
                         ?? new PagedList<ShopModel>([], new Metadata());

            var items = result.Items ?? [];

            if (!isFirstLoad)
                await SessionStorage.SetItemAsync(queryParameters);

            _tableGridClass = items.Count == 0
                ? CssClasses.GridTemplateEmptyClass
                : CssClasses.GridTemplateWithItemsClass + " grid-additional-columns";

            StateHasChanged();

            return new GridDataProviderResult<ShopModel>
            {
                Data = items,
                TotalCount = result.Metadata?.TotalCount ?? 0
            };
        }

        private async Task ShowErrorAsync(string message)
            => await MessageComponent!.ShowErrorAsync(message);

        private async Task ShowInfoAsync(string message)
            => await MessageComponent!.ShowInfoAsync(message);
    }
}