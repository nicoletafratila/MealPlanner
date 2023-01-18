using MealPlanner.UI.Web.Services;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    public partial class MealPlansOverview
    {
        public IList<MealPlanModel> MealPlans { get; set; } = new List<MealPlanModel>();

        [Inject]
        public IMealPlanService MealPlanService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            MealPlans = await MealPlanService.GetAll();
        }
    }
}
