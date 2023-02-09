using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    public partial class MealPlansOverview
    {
        public IList<MealPlanModel> MealPlans { get; set; } = new List<MealPlanModel>();
        public MealPlanModel CurrentMealPlanModel { get; set; } = new MealPlanModel();

        [Inject]
        public IMealPlanService MealPlanService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            MealPlans = await MealPlanService.GetAll();
        }

        protected void EditMealPlan(MealPlanModel item)
        {
            NavigationManager.NavigateTo($"mealplanedit/{item.Id}");
        }

        protected void NewMealPlan ()
        {
            NavigationManager.NavigateTo($"mealplanedit/");
        }
    }
}
