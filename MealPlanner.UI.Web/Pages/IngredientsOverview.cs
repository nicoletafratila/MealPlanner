using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class IngredientsOverview
    {
        public IList<IngredientModel> Ingredients { get; set; } = new List<IngredientModel>();

        [Inject]
        public IIngredientService IngredientService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Ingredients = await IngredientService.GetAll();
        }
    }
}
