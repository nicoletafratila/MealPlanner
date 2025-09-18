using BlazorBootstrap;
using Common.Constants;
using Common.Pagination;
using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    [Authorize]
    public partial class UnitsOverview
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem>? _navItems = default!;
        private GridTemplate<UnitModel>? _unitsGrid = default!;
        private string _tableGridClass = CssClasses.GridTemplateWithItemsClass;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Inject]
        public IUnitService? UnitsService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _navItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem{ Text = "Home", Href ="/" }
            };
            await RefreshAsync();
        }

        private void New()
        {
            NavigationManager?.NavigateTo($"unitedit/");
        }

        private void Update(UnitModel item)
        {
            NavigationManager?.NavigateTo($"unitedit/{item.Id}");
        }

        private async Task DeleteAsync(UnitModel item)
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

                var response = await UnitsService!.DeleteAsync(item.Id);
                if (response != null && !response.Succeeded)
                {
                    MessageComponent?.ShowError(response.Message!);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    await _unitsGrid!.RefreshDataAsync();
                }
            }
        }

        private async Task RefreshAsync()
        {
            var request = new GridDataProviderRequest<UnitModel>
            {
                Filters = new List<FilterItem>() { },
                Sorting = new List<SortingItem<UnitModel>>
                        {
                            new SortingItem<UnitModel>("Name", item => item.Name!, SortDirection.Ascending),
                        },
                PageNumber = 1,
                PageSize = 10
            };
            await UnitsDataProviderAsync(request);
        }

        private async Task<GridDataProviderResult<UnitModel>> UnitsDataProviderAsync(GridDataProviderRequest<UnitModel> request)
        {
            string sortString = "";
            SortDirection sortDirection = SortDirection.None;

            if (request.Sorting is not null && request.Sorting.Any())
            {
                sortString = request.Sorting.FirstOrDefault()!.SortString;
                sortDirection = request.Sorting.FirstOrDefault()!.SortDirection;
            }
            var queryParameters = new QueryParameters()
            {
                Filters = request.Filters,
                SortString = sortString,
                SortDirection = sortDirection,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };

            var result = await UnitsService!.SearchAsync(queryParameters);
            if (result == null || result.Items == null)
            {
                result = new PagedList<UnitModel>(new List<UnitModel>(), new Metadata());
            }
            _tableGridClass = result!.Items!.Any() ? CssClasses.GridTemplateWithItemsClass : CssClasses.GridTemplateEmptyClass;
            return await Task.FromResult(new GridDataProviderResult<UnitModel> { Data = result!.Items, TotalCount = result.Metadata!.TotalCount });
        }
    }
}
