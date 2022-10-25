using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class RecipesOverview
    {
        public IEnumerable<RecipeModel> Recipes { get; set; } = new List<RecipeModel>();

        [Inject]
        public IRecipeService RecipeService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Recipes = await RecipeService.GetAll();
        }
    }
}
