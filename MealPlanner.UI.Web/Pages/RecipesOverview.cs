using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class RecipesOverview
    {
        public IList<RecipeModel> Recipes { get; set; } = new List<RecipeModel>();

        public RecipeModel CurrentRecipeModel { get; set; } = new RecipeModel();

        [Inject]
        public IRecipeService RecipeService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Recipes = await RecipeService.GetAll();
        }

        protected void EditRecipe(RecipeModel item)
        {
            NavigationManager.NavigateTo($"recipeedit/{item.Id}");
        }

        protected void NewRecipe()
        {
            NavigationManager.NavigateTo($"recipeedit/");
        }
    }
}
