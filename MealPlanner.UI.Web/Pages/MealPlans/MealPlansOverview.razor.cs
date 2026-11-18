using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Constants;
using Common.Pagination;
using Common.UI;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.MealPlans
{
    [Authorize]
    public partial class MealPlansOverview
    {
        private const string CreatedAfterFilterStorageKey = "MealPlansOverviewCreatedAfterFilter";

        private ConfirmDialog _dialog = default!;
        private GridTemplate<MealPlanModel>? _mealPlansGrid;
        private string _tableGridClass = CssClasses.GridTemplateEmptyClass + " grid-additional-columns";
        private BlazorBootstrap.SortDirection _createdAtSortDirection = BlazorBootstrap.SortDirection.Descending;
        private int _gridKey = 0;
        private bool _firstLoad = true;

        private DateTime? CreatedAfterFilter { get; set; }

        private string CreatedAtSortIconClass => _createdAtSortDirection switch
        {
            BlazorBootstrap.SortDirection.Ascending => "bi bi-sort-alpha-down",
            BlazorBootstrap.SortDirection.Descending => "bi bi-sort-alpha-down-alt",
            _ => "bi bi-arrow-down-up"
        };

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Inject]
        public IMealPlanService MealPlanService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public ISessionStorageService SessionStorage { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            CreatedAfterFilter = await SessionStorage.GetItemAsync<DateTime?>(name: CreatedAfterFilterStorageKey);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            var stored = await SessionStorage.GetItemAsync<QueryParameters<MealPlanModel>>();
            var createdAtSort = stored?.Sorting?.FirstOrDefault(s => s.PropertyName == "CreatedAt");
            var direction = (BlazorBootstrap.SortDirection)(int)(createdAtSort?.Direction ?? Common.Pagination.SortDirection.Descending);

            var sortChanged = direction != _createdAtSortDirection;
            if (sortChanged)
                _createdAtSortDirection = direction;

            if (sortChanged || CreatedAfterFilter.HasValue)
            {
                _gridKey++;
                StateHasChanged();
            }
        }

        private async Task OnCreatedAtSortClickedAsync()
        {
            _createdAtSortDirection = _createdAtSortDirection switch
            {
                BlazorBootstrap.SortDirection.Ascending => BlazorBootstrap.SortDirection.Descending,
                BlazorBootstrap.SortDirection.Descending => BlazorBootstrap.SortDirection.None,
                _ => BlazorBootstrap.SortDirection.Ascending
            };

            var stored = await SessionStorage.GetItemAsync<QueryParameters<MealPlanModel>>();
            var resetParameters = new QueryParameters<MealPlanModel>
            {
                Filters = stored?.Filters,
                Sorting = [new SortingModel { PropertyName = "CreatedAt", Direction = (Common.Pagination.SortDirection)(int)_createdAtSortDirection }],
                PageNumber = 1,
                PageSize = stored?.PageSize ?? new QueryParameters<MealPlanModel>().PageSize
            };
            await SessionStorage.SetItemAsync(resetParameters);

            _gridKey++;
            StateHasChanged();
        }

        private async Task OnCreatedAfterDateChangedAsync(ChangeEventArgs e)
        {
            CreatedAfterFilter = DateTime.TryParse(e.Value?.ToString(), out var date) ? date : null;
            await OnCreatedAfterFilterChangedAsync();
        }

        private async Task OnCreatedAfterFilterChangedAsync()
        {
            await SessionStorage.SetItemAsync(CreatedAfterFilter, CreatedAfterFilterStorageKey);

            var stored = await SessionStorage.GetItemAsync<QueryParameters<MealPlanModel>>();
            var resetParameters = new QueryParameters<MealPlanModel>
            {
                Filters = stored?.Filters,
                Sorting = stored?.Sorting ?? [],
                PageNumber = 1,
                PageSize = stored?.PageSize ?? new QueryParameters<MealPlanModel>().PageSize
            };
            await SessionStorage.SetItemAsync(resetParameters);

            _gridKey++;
            StateHasChanged();
        }

        private void New()
        {
            NavigationManager.NavigateTo("mealplans/mealplanedit/");
        }

        private void Update(MealPlanModel item)
        {
            if (item is null)
                return;

            NavigationManager.NavigateTo($"mealplans/mealplanedit/{item.Id}");
        }

        private async Task DeleteAsync(MealPlanModel item)
        {
            if (item is null)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = Resources.MealPlansOverview.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.MealPlansOverview.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.MealPlansOverview.DeleteDialogTitle,
                message1: Resources.MealPlansOverview.DeleteDialogMessage1,
                message2: Resources.MealPlansOverview.DeleteDialogMessage2,
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            await DeleteCoreAsync(item);
        }

        private async Task DeleteCoreAsync(MealPlanModel item)
        {
            var response = await MealPlanService.DeleteAsync(item.Id);
            if (response is null)
            {
                await ShowErrorAsync(Resources.MealPlansOverview.DeleteFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.MealPlansOverview.DeleteFailed);
                return;
            }

            await ShowInfoAsync(Resources.MealPlansOverview.DeleteSucceeded);

            if (_mealPlansGrid is not null)
                await _mealPlansGrid.RefreshDataAsync();
        }

        private async Task<GridSettings?> SettingsProviderAsync()
        {
            var qp = await SessionStorage.GetItemAsync<QueryParameters<MealPlanModel>>();
            if (qp is null)
                return null;

            return new GridSettings
            {
                PageNumber = qp.PageNumber,
                PageSize = qp.PageSize,
                Filters = qp.Filters?
                    .Select(f => new BlazorBootstrap.FilterItem(f.PropertyName, f.Value?.ToString() ?? string.Empty, (BlazorBootstrap.FilterOperator)(int)f.Operator, f.StringComparison))
                    .ToList()
            };
        }

        private async Task<GridDataProviderResult<MealPlanModel>> DataProviderAsync(
            GridDataProviderRequest<MealPlanModel> request)
        {
            var filters = request.Filters?
                .Select(f => new Common.Pagination.FilterItem(f.PropertyName, f.Value, (Common.Pagination.FilterOperator)(int)f.Operator, f.StringComparison))
                .ToList() ?? [];

            AddCustomFilters(filters);

            var queryParameters = new QueryParameters<MealPlanModel>
            {
                Filters = filters,
                Sorting = request.Sorting?
                    .Select(s => new SortingModel { PropertyName = s.SortString, Direction = (Common.Pagination.SortDirection)(int)s.SortDirection })
                    .ToList() ?? [],
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };

            var isFirstLoad = _firstLoad;
            _firstLoad = false;

            var result = await MealPlanService.SearchAsync(queryParameters)
                         ?? new PagedList<MealPlanModel>([], new Metadata());

            var items = result.Items ?? [];

            if (!isFirstLoad)
            {
                var sessionParameters = new QueryParameters<MealPlanModel>
                {
                    Filters = request.Filters?
                        .Select(f => new Common.Pagination.FilterItem(f.PropertyName, f.Value, (Common.Pagination.FilterOperator)(int)f.Operator, f.StringComparison))
                        .ToList(),
                    Sorting = queryParameters.Sorting,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
                await SessionStorage.SetItemAsync(sessionParameters);
            }

            _tableGridClass = (items.Count == 0
                ? CssClasses.GridTemplateEmptyClass
                : CssClasses.GridTemplateWithItemsClass) + " grid-additional-columns";

            StateHasChanged();

            return new GridDataProviderResult<MealPlanModel>
            {
                Data = items,
                TotalCount = result.Metadata?.TotalCount ?? 0
            };
        }

        private void AddCustomFilters(List<Common.Pagination.FilterItem> filters)
        {
            if (CreatedAfterFilter.HasValue)
                filters.Add(new Common.Pagination.FilterItem("CreatedAt", CreatedAfterFilter.Value.Date, Common.Pagination.FilterOperator.GreaterThanOrEquals));
        }

        private async Task ShowErrorAsync(string message)
            => await MessageComponent!.ShowErrorAsync(message);

        private async Task ShowInfoAsync(string message)
            => await MessageComponent!.ShowInfoAsync(message);
    }
}