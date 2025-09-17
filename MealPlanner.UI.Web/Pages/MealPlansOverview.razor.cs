using BlazorBootstrap;
using Common.Constants;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    [Authorize]
    public partial class MealPlansOverview
    {
        private List<BreadcrumbItem>? navItems { get; set; }
        private ConfirmDialog dialog = default!;
        private GridTemplate<MealPlanModel>? mealPlansGrid = default!;
        private string tableGridClass { get; set; } = CssClasses.GridTemplateWithItemsClass;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? messageComponent { get; set; }

        [Inject]
        public IMealPlanService? MealPlanService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            navItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem{ Text = "Home", Href ="/" }
            };

            await RefreshAsync();
        }

        private void New()
        {
            NavigationManager?.NavigateTo($"mealplanedit/");
        }

        private void Update(MealPlanModel item)
        {
            NavigationManager?.NavigateTo($"mealplanedit/{item.Id}");
        }

        private async Task DeleteAsync(MealPlanModel item)
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
                var confirmation = await dialog.ShowAsync(
                        title: "Are you sure you want to delete this?",
                        message1: "This will delete the record. Once deleted can not be rolled back.",
                        message2: "Do you want to proceed?",
                        confirmDialogOptions: options);

                if (!confirmation)
                    return;

                var response = await MealPlanService!.DeleteAsync(item.Id);
                if (response != null && !response.Succeeded)
                {
                    messageComponent?.ShowError(response.Message!);
                }
                else
                {
                    messageComponent?.ShowInfo("Data has been deleted successfully");
                    await mealPlansGrid!.RefreshDataAsync();
                }
            }
        }

        private async Task RefreshAsync()
        {
            var request = new GridDataProviderRequest<MealPlanModel>
            {
                Filters = new List<FilterItem>() { },
                Sorting = new List<SortingItem<MealPlanModel>>
                        {
                            new SortingItem<MealPlanModel>("Name", item => item.Name!, SortDirection.Ascending),
                        },
                PageNumber = 1,
                PageSize = 10
            };
            await MealPlansDataProviderAsync(request);
        }

        private async Task<GridDataProviderResult<MealPlanModel>> MealPlansDataProviderAsync(GridDataProviderRequest<MealPlanModel> request)
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

            var result = await MealPlanService!.SearchAsync(queryParameters);
            if (result == null || result.Items == null)
            {
                result = new PagedList<MealPlanModel>(new List<MealPlanModel>(), new Metadata());
            }
            tableGridClass = result!.Items!.Any() ? CssClasses.GridTemplateWithItemsClass : CssClasses.GridTemplateEmptyClass;
            return await Task.FromResult(new GridDataProviderResult<MealPlanModel> { Data = result!.Items, TotalCount = result.Metadata!.TotalCount });
        }
    }
}
