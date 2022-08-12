using MealPlanner.App.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.App.Pages
{
    public partial class MealPlanEdit
    {
        [Inject]
        public IMealPlanService MealPlanService { get; set; }

        [Parameter]
        public string Id { get; set; }

        public MealPlanModel Model { get; set; } = new MealPlanModel();

        protected bool Saved;

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);
            Saved = false;

            if (id == 0)
            {
                Model = new MealPlanModel();
            }
            else
            {
                Model = await MealPlanService.Get(int.Parse(Id));
            }
        }

        protected async Task Save()
        {
            Saved = false;
        }
    }
}
