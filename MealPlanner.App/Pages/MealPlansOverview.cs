using MealPlanner.App.Services;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.App.Pages
{
    public partial class MealPlansOverview
    {
        public IEnumerable<MealPlanModel> MealPlans { get; set; } = new List<MealPlanModel>();

        [Inject]
        public IMealPlanService MealPlanService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            MealPlans = await MealPlanService.GetAll();
        }
    }
}
