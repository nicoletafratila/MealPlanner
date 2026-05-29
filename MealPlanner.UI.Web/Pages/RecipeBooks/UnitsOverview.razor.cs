using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Constants;
using Common.Pagination;
using Common.UI;
using Identity.Services.Core;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Services.Core;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages.RecipeBooks
{
    [Authorize]
    public partial class UnitsOverview
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = [];
        private GridTemplate<UnitModel>? _unitsGrid;
        private string _tableGridClass = CssClasses.GridTemplateEmptyClass;
        private BlazorBootstrap.SortDirection _nameSortDirection = BlazorBootstrap.SortDirection.Ascending;
        private int _gridKey = 0;
        private bool _firstLoad = true;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Inject]
        public IUnitService UnitsService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public ISessionStorageService SessionStorage { get; set; } = default!;

        protected override void OnInitialized()
        {
            _navItems =
            [
                new BreadcrumbItem { Text = Resources.UnitsOverview.BreadcrumbHome, Href = "recipebooks/recipesoverview" }
            ];
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            var stored = await SessionStorage.GetItemAsync<QueryParameters<UnitModel>>();
            var nameSort = stored?.Sorting?.FirstOrDefault(s => s.PropertyName == "Name");
            var direction = (BlazorBootstrap.SortDirection)(int)(nameSort?.Direction ?? Common.Pagination.SortDirection.Ascending);

            if (direction != _nameSortDirection)
            {
                _nameSortDirection = direction;
                _gridKey++;
                StateHasChanged();
            }
        }

        private async Task<GridSettings?> SettingsProviderAsync()
        {
            var qp = await SessionStorage.GetItemAsync<QueryParameters<UnitModel>>();
            return qp is null ? null : new GridSettings { PageNumber = qp.PageNumber, PageSize = qp.PageSize };
        }

        private void New()
        {
            NavigationManager.NavigateTo("recipebooks/unitedit/");
        }

        private void Update(UnitModel item)
        {
            if (item is null)
                return;

            NavigationManager.NavigateTo($"recipebooks/unitedit/{item.Id}");
        }

        private async Task DeleteAsync(UnitModel item)
        {
            if (item is null)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = Resources.UnitsOverview.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.UnitsOverview.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.UnitsOverview.DeleteDialogTitle,
                message1: Resources.UnitsOverview.DeleteDialogMessage1,
                message2: Resources.UnitsOverview.DeleteDialogMessage2,
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            await DeleteCoreAsync(item);
        }

        private async Task DeleteCoreAsync(UnitModel item)
        {
            var response = await UnitsService.DeleteAsync(item.Id);
            if (response is null)
            {
                await ShowErrorAsync(Resources.UnitsOverview.DeleteFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.UnitsOverview.DeleteFailed);
                return;
            }

            await ShowInfoAsync(Resources.UnitsOverview.DeleteSucceeded);

            if (_unitsGrid is not null)
                await _unitsGrid.RefreshDataAsync();
        }

        private async Task<GridDataProviderResult<UnitModel>> DataProviderAsync(GridDataProviderRequest<UnitModel> request)
        {
            var queryParameters = new QueryParameters<UnitModel>
            {
                Filters = request.Filters?
                    .Select(f => new Common.Pagination.FilterItem(f.PropertyName, f.Value, (Common.Pagination.FilterOperator)(int)f.Operator, f.StringComparison))
                    .ToList(),
                Sorting = request.Sorting?
                    .Select(s => new SortingModel { PropertyName = s.SortString, Direction = (Common.Pagination.SortDirection)(int)s.SortDirection })
                    .ToList() ?? [],
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var isFirstLoad = _firstLoad;
            _firstLoad = false;

            var result = await UnitsService.SearchAsync(queryParameters) ?? new PagedList<UnitModel>([], new Metadata());
            var items = result.Items ?? [];

            if (!isFirstLoad)
                await SessionStorage.SetItemAsync(queryParameters);

            _tableGridClass = items.Count == 0
                ? CssClasses.GridTemplateEmptyClass
                : CssClasses.GridTemplateWithItemsClass + " grid-additional-columns";

            StateHasChanged();
            return new GridDataProviderResult<UnitModel>
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