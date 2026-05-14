using BlazorBootstrap;
using Common.Constants;
using Common.Pagination;
using Identity.Shared.Models;
using MealPlanner.UI.Web.Services.Identities;
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

        [Inject]
        public IApplicationUserService ApplicationUserService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        protected override void OnInitialized()
        {
            _navItems =
            [
                new BreadcrumbItem { Text = Resources.UsersOverview.BreadcrumbHome, Href = "recipebooks/recipesoverview" }
            ];
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
                Filters = request.Filters,
                Sorting = request.Sorting?
                    .Select(QueryParameters<ApplicationUserModel>.ToModel)
                    .ToList()!,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };

            var result = await ApplicationUserService.SearchAsync(queryParameters)
                         ?? new PagedList<ApplicationUserModel>([], new Metadata());

            var items = result.Items ?? [];

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
