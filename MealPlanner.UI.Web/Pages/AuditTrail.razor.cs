using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Constants;
using Common.Models;
using Common.Pagination;
using Common.Services;
using Common.UI;
using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    [Authorize(Roles = "admin")]
    public partial class AuditTrail
    {
        private static readonly string[] LevelOptions = ["Verbose", "Debug", "Information", "Warning", "Error", "Fatal"];

        private ConfirmDialog _dialog = default!;
        private GridTemplate<LogModel>? _logsGrid;
        private string _tableGridClass = CssClasses.GridTemplateEmptyClass + " audit-columns";
        private BlazorBootstrap.SortDirection _timestampSortDirection = BlazorBootstrap.SortDirection.Descending;
        private int _gridKey = 0;
        private bool _firstLoad = true;

        private string? LevelFilter { get; set; }

        private DateTime? TimestampFromFilter { get; set; }

        private DateTime? TimestampToFilter { get; set; }

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Inject]
        public ILoggerService LoggerService { get; set; } = default!;

        [Inject]
        public ILogger<AuditTrail> Logger { get; set; } = default!;

        [Inject]
        public ISessionStorageService SessionStorage { get; set; } = default!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            var stored = await SessionStorage.GetItemAsync<QueryParameters<LogModel>>();
            var timestampSort = stored?.Sorting?.FirstOrDefault(s => s.PropertyName == "Timestamp");
            var direction = (BlazorBootstrap.SortDirection)(int)(timestampSort?.Direction ?? Common.Pagination.SortDirection.Descending);

            if (direction != _timestampSortDirection)
            {
                _timestampSortDirection = direction;
                _gridKey++;
                StateHasChanged();
            }
        }

        private async Task<GridSettings?> SettingsProviderAsync()
        {
            var qp = await SessionStorage.GetItemAsync<QueryParameters<LogModel>>();
            return qp is null ? null : new GridSettings { PageNumber = qp.PageNumber, PageSize = qp.PageSize };
        }

        private async Task OnFromDateChangedAsync(ChangeEventArgs e)
        {
            TimestampFromFilter = DateTime.TryParse(e.Value?.ToString(), out var date) ? date : null;
            await OnCustomFilterChangedAsync();
        }

        private async Task OnToDateChangedAsync(ChangeEventArgs e)
        {
            TimestampToFilter = DateTime.TryParse(e.Value?.ToString(), out var date) ? date : null;
            await OnCustomFilterChangedAsync();
        }

        private async Task OnCustomFilterChangedAsync()
        {
            var stored = await SessionStorage.GetItemAsync<QueryParameters<LogModel>>();
            var resetParameters = new QueryParameters<LogModel>
            {
                Filters = stored?.Filters,
                Sorting = stored?.Sorting ?? [],
                PageNumber = 1,
                PageSize = stored?.PageSize ?? new QueryParameters<LogModel>().PageSize
            };
            await SessionStorage.SetItemAsync(resetParameters);

            _gridKey++;
            StateHasChanged();
        }

        private async Task DeleteAllLogsAsync()
        {
            var options = new ConfirmDialogOptions
            {
                YesButtonText = Resources.AuditTrail.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.AuditTrail.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.AuditTrail.DeleteDialogTitle,
                message1: Resources.AuditTrail.DeleteDialogMessage1,
                message2: Resources.AuditTrail.DeleteDialogMessage2,
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            await DeleteAllLogsCoreAsync();
        }

        private async Task DeleteAllLogsCoreAsync()
        {
            try
            {
                await LoggerService.DeleteLogsAsync();
                await ShowInfoAsync(Resources.AuditTrail.DeleteSucceeded);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to delete audit trail logs.");
                await ShowErrorAsync(Resources.AuditTrail.DeleteFailedMessage);
                return;
            }

            if (_logsGrid is not null)
                await _logsGrid.RefreshDataAsync();
        }

        private async Task<GridDataProviderResult<LogModel>> DataProviderAsync(GridDataProviderRequest<LogModel> request)
        {
            var filters = request.Filters?
                .Select(f => new Common.Pagination.FilterItem(f.PropertyName, f.Value, (Common.Pagination.FilterOperator)(int)f.Operator, f.StringComparison))
                .ToList() ?? [];

            AddCustomFilters(filters);

            var queryParameters = new QueryParameters<LogModel>
            {
                Filters = filters,
                Sorting = request.Sorting?
                    .Select(s => new SortingModel { PropertyName = s.SortString, Direction = (Common.Pagination.SortDirection)(int)s.SortDirection })
                    .ToList() ?? [],
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var isFirstLoad = _firstLoad;
            _firstLoad = false;

            PagedList<LogModel> result;
            try
            {
                result = await LoggerService.SearchAsync(queryParameters);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to load audit trail logs.");
                result = new PagedList<LogModel>([], new Metadata());
            }

            var items = result.Items ?? [];

            if (!isFirstLoad)
            {
                var sessionParameters = new QueryParameters<LogModel>
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
                : CssClasses.GridTemplateWithItemsClass) + " audit-columns";

            StateHasChanged();
            return new GridDataProviderResult<LogModel>
            {
                Data = items,
                TotalCount = result.Metadata?.TotalCount ?? 0
            };
        }

        private void AddCustomFilters(List<Common.Pagination.FilterItem> filters)
        {
            if (!string.IsNullOrWhiteSpace(LevelFilter))
                filters.Add(new Common.Pagination.FilterItem("Level", LevelFilter, Common.Pagination.FilterOperator.Equals, StringComparison.OrdinalIgnoreCase));

            if (TimestampFromFilter.HasValue)
                filters.Add(new Common.Pagination.FilterItem("Timestamp", TimestampFromFilter.Value.Date, Common.Pagination.FilterOperator.GreaterThanOrEquals));

            if (TimestampToFilter.HasValue)
                filters.Add(new Common.Pagination.FilterItem("Timestamp", TimestampToFilter.Value.Date.AddDays(1).AddTicks(-1), Common.Pagination.FilterOperator.LessThanOrEquals));
        }

        private async Task ShowErrorAsync(string message)
            => await MessageComponent!.ShowErrorAsync(message);

        private async Task ShowInfoAsync(string message)
            => await MessageComponent!.ShowInfoAsync(message);
    }
}
