using MealPlanner.App.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

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
