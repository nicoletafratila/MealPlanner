using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MealPlanner.UI.Web.Pages
{
    public partial class MealPlansOverview
    {
        [Parameter]
        public QueryParameters? QueryParameters { get; set; } = new();

        public MealPlanModel? MealPlan { get; set; }
        public PagedList<MealPlanModel>? MealPlans { get; set; }

        [Inject]
        public IMealPlanService? MealPlanService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        [CascadingParameter(Name = "ErrorComponent")]
        protected IErrorComponent? ErrorComponent { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await RefreshAsync();
        }

        private void New()
        {
            NavigationManager!.NavigateTo($"mealplanedit/");
        }

        private void Update(MealPlanModel item)
        {
            NavigationManager!.NavigateTo($"mealplanedit/{item.Id}");
        }

        private async void DeleteAsync(MealPlanModel item)
        {
            if (item != null)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the meal plan: '{item.Name}'?"))
                    return;

                var response = await MealPlanService!.DeleteAsync(item.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    ErrorComponent!.ShowError("Error", response);
                }
                else
                {
                    await RefreshAsync();
                }
            }
        }

        private async Task RefreshAsync()
        {
            MealPlans = await MealPlanService!.SearchAsync(QueryParameters!);
            StateHasChanged();
        }

        private async void OnPageChangedAsync(int pageNumber)
        {
            QueryParameters!.PageNumber = pageNumber;
            await RefreshAsync();
        }
    }
}
