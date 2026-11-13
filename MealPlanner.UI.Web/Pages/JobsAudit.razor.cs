using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Constants;
using Common.Pagination;
using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Enums;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    [Authorize(Roles = "admin")]
    public partial class JobsAudit
    {
        private static readonly string[] StatusOptions = ["Pending", "Running", "Completed", "Failed"];

        private GridTemplate<JobModel>? _jobsGrid;
        private string _tableGridClass = CssClasses.GridTemplateEmptyClass;
        private BlazorBootstrap.SortDirection _createdAtSortDirection = BlazorBootstrap.SortDirection.Descending;
        private int _gridKey = 0;
        private bool _firstLoad = true;

        private string? StatusFilter { get; set; }

        private DateTime? CreatedFromFilter { get; set; }

        private DateTime? CreatedToFilter { get; set; }

        [Inject]
        public IJobService JobService { get; set; } = default!;

        [Inject]
        public ILogger<JobsAudit> Logger { get; set; } = default!;

        [Inject]
        public ISessionStorageService SessionStorage { get; set; } = default!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            var stored = await SessionStorage.GetItemAsync<QueryParameters<JobModel>>();
            var createdAtSort = stored?.Sorting?.FirstOrDefault(s => s.PropertyName == "CreatedAt");
            var direction = (BlazorBootstrap.SortDirection)(int)(createdAtSort?.Direction ?? Common.Pagination.SortDirection.Descending);

            if (direction != _createdAtSortDirection)
            {
                _createdAtSortDirection = direction;
                _gridKey++;
                StateHasChanged();
            }
        }

        private async Task<GridSettings?> SettingsProviderAsync()
        {
            var qp = await SessionStorage.GetItemAsync<QueryParameters<JobModel>>();
            return qp is null ? null : new GridSettings { PageNumber = qp.PageNumber, PageSize = qp.PageSize };
        }

        private async Task OnFromDateChangedAsync(ChangeEventArgs e)
        {
            CreatedFromFilter = DateTime.TryParse(e.Value?.ToString(), out var date) ? date : null;
            await OnCustomFilterChangedAsync();
        }

        private async Task OnToDateChangedAsync(ChangeEventArgs e)
        {
            CreatedToFilter = DateTime.TryParse(e.Value?.ToString(), out var date) ? date : null;
            await OnCustomFilterChangedAsync();
        }

        private async Task OnCustomFilterChangedAsync()
        {
            var stored = await SessionStorage.GetItemAsync<QueryParameters<JobModel>>();
            var resetParameters = new QueryParameters<JobModel>
            {
                Filters = stored?.Filters,
                Sorting = stored?.Sorting ?? [],
                PageNumber = 1,
                PageSize = stored?.PageSize ?? new QueryParameters<JobModel>().PageSize
            };
            await SessionStorage.SetItemAsync(resetParameters);

            _gridKey++;
            StateHasChanged();
        }

        private async Task<GridDataProviderResult<JobModel>> DataProviderAsync(GridDataProviderRequest<JobModel> request)
        {
            var filters = request.Filters?
                .Select(f => new Common.Pagination.FilterItem(f.PropertyName, f.Value, (Common.Pagination.FilterOperator)(int)f.Operator, f.StringComparison))
                .ToList() ?? [];

            AddCustomFilters(filters);

            var queryParameters = new QueryParameters<JobModel>
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

            PagedList<JobModel> result;
            try
            {
                result = await JobService.SearchAsync(queryParameters) ?? new PagedList<JobModel>([], new Metadata());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to load job history.");
                result = new PagedList<JobModel>([], new Metadata());
            }

            var items = result.Items ?? [];

            if (!isFirstLoad)
            {
                var sessionParameters = new QueryParameters<JobModel>
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

            _tableGridClass = items.Count == 0
                ? CssClasses.GridTemplateEmptyClass
                : CssClasses.GridTemplateWithItemsClass;

            StateHasChanged();
            return new GridDataProviderResult<JobModel>
            {
                Data = items,
                TotalCount = result.Metadata?.TotalCount ?? 0
            };
        }

        private void AddCustomFilters(List<Common.Pagination.FilterItem> filters)
        {
            if (!string.IsNullOrWhiteSpace(StatusFilter) && Enum.TryParse<JobStatus>(StatusFilter, out var status))
                filters.Add(new Common.Pagination.FilterItem("Status", status.ToString(), Common.Pagination.FilterOperator.Equals));

            if (CreatedFromFilter.HasValue)
                filters.Add(new Common.Pagination.FilterItem("CreatedAt", CreatedFromFilter.Value.Date, Common.Pagination.FilterOperator.GreaterThanOrEquals));

            if (CreatedToFilter.HasValue)
                filters.Add(new Common.Pagination.FilterItem("CreatedAt", CreatedToFilter.Value.Date.AddDays(1).AddTicks(-1), Common.Pagination.FilterOperator.LessThanOrEquals));
        }
    }
}
