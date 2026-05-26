using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Constants;
using Common.Pagination;
using Identity.Shared.Models;
using Identity.Services;
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
        private SortDirection _nameSortDirection = SortDirection.Ascending;
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
            var direction = nameSort?.Direction ?? SortDirection.Ascending;

            if (direction != _nameSortDirection)
            {
                _nameSortDirection = direction;
                _gridKey++;
                StateHasChanged();
            }
        }

        private async Task<GridSettings?> SettingsProviderAsync()
            => await SessionStorage.GetItemAsync<QueryParameters<ApplicationUserModel>>();

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
                Filters = request.Filters,
                Sorting = request.Sorting?
                    .Select(QueryParameters<ApplicationUserModel>.ToModel)
                    .ToList()!,
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
