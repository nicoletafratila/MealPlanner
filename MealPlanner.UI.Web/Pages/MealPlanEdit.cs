using MealPlanner.UI.Web.Services;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class MealPlanEdit
    {
        [Parameter]
        public string Id { get; set; }

        public EditMealPlanModel Model { get; set; } = new EditMealPlanModel();
        public RecipeModel CurrentRecipeModel { get; set; } = new RecipeModel();

        [Inject]
        public IMealPlanService MealPlanService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);

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
        }

        protected async Task NavigateToOverview()
        {
            NavigationManager.NavigateTo("/mealplansoverview");
        }

        protected async Task ShowShoppingList()
        {
            NavigationManager.NavigateTo($"shoppinglist/{Model.Id}");
        }

        protected async Task EditRecipe(RecipeModel item)
        {
            NavigationManager.NavigateTo($"recipeedit/{item.Id}");
        }
    }
}
