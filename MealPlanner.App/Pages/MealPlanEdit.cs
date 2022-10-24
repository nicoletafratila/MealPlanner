using MealPlanner.App.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.App.Pages
{
    public partial class MealPlanEdit
    {
        [Inject]
        public IMealPlanService MealPlanService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Parameter]
        public string Id { get; set; }

        public EditMealPlanModel Model { get; set; } = new EditMealPlanModel();

        protected bool Saved;

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);
            Saved = false;

            if (id == 0)
            {
                Model = new EditMealPlanModel();
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

        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo("/mealplansoverview");
        }
    }
}
