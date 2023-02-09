using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class IngredientsOverview
    {
        public IList<IngredientModel> Ingredients { get; set; } = new List<IngredientModel>();
        public IngredientModel CurrentIngredientModel { get; set; } = new IngredientModel();

        [Inject]
        public IIngredientService IngredientService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Ingredients = await IngredientService.GetAll();
        }

        protected void EditIngredient(IngredientModel item)
        {
            NavigationManager.NavigateTo($"ingredientedit/{item.Id}");
        }

        protected void NewIngredient()
        {
            NavigationManager.NavigateTo($"ingredientedit/");
        }
    }
}
