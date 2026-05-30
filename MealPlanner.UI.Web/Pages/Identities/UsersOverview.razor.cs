using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Constants;
using Common.Pagination;
using MealPlanner.UI.Web.Services;
using Identity.Services.Http;
using Identity.Shared.Models;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.Identities
{
    [Authorize(Roles = "admin")]
    public partial class UsersOverview
    {
        private List<BreadcrumbItem> _navItems = [];
        private GridTemplate<ApplicationUserModel>? _usersGrid;
        private string _tableGridClass = CssClasses.GridTemplateEmptyClass;
        private BlazorBootstrap.SortDirection _nameSortDirection = BlazorBootstrap.SortDirection.Ascending;
        private int _gridKey = 0;
        private bool _firstLoad = true;

        [Inject]
        public IApplicationUserService ApplicationUserService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public ISessionStorageService SessionStorage { get; set; } = default!;

        protected override void OnInitialized()
        {
            _navItems =
            [
                new BreadcrumbItem { Text = Resources.UsersOverview.BreadcrumbHome, Href = "recipebooks/recipesoverview" }
            ];
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            var stored = await SessionStorage.GetItemAsync<QueryParameters<ApplicationUserModel>>();
            var nameSort = stored?.Sorting?.FirstOrDefault(s => s.PropertyName == "Username");
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
            var qp = await SessionStorage.GetItemAsync<QueryParameters<ApplicationUserModel>>();
            return qp is null ? null : new GridSettings { PageNumber = qp.PageNumber, PageSize = qp.PageSize };
        }

        private void Update(ApplicationUserModel item)
        {
            if (item is null)
                return;

            NavigationManager.NavigateTo($"identities/userprofile/{item.Username}");
        }

        private async Task<GridDataProviderResult<ApplicationUserModel>> DataProviderAsync(
            GridDataProviderRequest<ApplicationUserModel> request)
        {
            var queryParameters = new QueryParameters<ApplicationUserModel>
            {
                Filters = request.Filters?
                    .Select(f => new Common.Pagination.FilterItem(f.PropertyName, f.Value, (Common.Pagination.FilterOperator)(int)f.Operator, f.StringComparison))
                    .ToList(),
                Sorting = request.Sorting?
                    .Select(s => new SortingModel { PropertyName = s.SortString, Direction = (Common.Pagination.SortDirection)(int)s.SortDirection })
                    .ToList() ?? [],
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };

            var isFirstLoad = _firstLoad;
            _firstLoad = false;

            var result = await ApplicationUserService.SearchAsync(queryParameters)
                         ?? new PagedList<ApplicationUserModel>([], new Metadata());

            var items = result.Items ?? [];

            if (!isFirstLoad)
                await SessionStorage.SetItemAsync(queryParameters);

            _tableGridClass = items.Count == 0
                ? CssClasses.GridTemplateEmptyClass
                : CssClasses.GridTemplateWithItemsClass + " grid-additional-columns";

            StateHasChanged();

            return new GridDataProviderResult<ApplicationUserModel>
            {
                Data = items,
                TotalCount = result.Metadata?.TotalCount ?? 0
            };
        }
    }
}
