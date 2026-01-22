using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Constants;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Services.MealPlans;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.MealPlans
{
    [Authorize]
    public partial class MealPlansOverview
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = default!;
        private GridTemplate<MealPlanModel>? _mealPlansGrid = default!;
        private string _tableGridClass = CssClasses.GridTemplateEmptyClass;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Inject]
        public IMealPlanService? MealPlanService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public ISessionStorageService? SessionStorage { get; set; }

        protected override void OnInitialized()
        {
            _navItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem{ Text = "Home", Href ="recipebooks/recipesoverview" }
            };
        }

        private void New()
        {
            NavigationManager?.NavigateTo($"mealplans/mealplanedit/");
        }

        private void Update(MealPlanModel item)
        {
            NavigationManager?.NavigateTo($"mealplans/mealplanedit/{item.Id}");
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
                var confirmation = await _dialog.ShowAsync(
                        title: "Are you sure you want to delete this?",
                        message1: "This will delete the record. Once deleted can not be rolled back.",
                        message2: "Do you want to proceed?",
                        confirmDialogOptions: options);

                if (!confirmation)
                    return;

                var response = await MealPlanService!.DeleteAsync(item.Id);
                if (response != null && !response.Succeeded)
                {
                    MessageComponent?.ShowError(response.Message!);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    await _mealPlansGrid!.RefreshDataAsync();
                }
            }
        }

        private async Task<GridDataProviderResult<MealPlanModel>> DataProviderAsync(GridDataProviderRequest<MealPlanModel> request)
        {
            var queryParameters = new QueryParameters<MealPlanModel>()
            {
                Filters = request.Filters,
                Sorting = request.Sorting?.Select(x => QueryParameters<MealPlanModel>.ToModel(x)).ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };

            var result = await MealPlanService!.SearchAsync(queryParameters);
            if (result == null || result.Items == null)
            {
                result = new PagedList<MealPlanModel>(new List<MealPlanModel>(), new Metadata());
            }
            await SessionStorage!.SetItemAsync(queryParameters);
            _tableGridClass = result!.Items!.Count == 0 ? CssClasses.GridTemplateEmptyClass : CssClasses.GridTemplateWithItemsClass + " grid-additional-columns"; ;
            StateHasChanged();
            return new GridDataProviderResult<MealPlanModel> { Data = result!.Items, TotalCount = result.Metadata!.TotalCount };
        }
    }
}
