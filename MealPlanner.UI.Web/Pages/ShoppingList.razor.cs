using MealPlanner.UI.Web.Services;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ShoppingList
    {
        [Parameter]
        public string Id { get; set; }

        public ShoppingListModel Model { get; set; } = new ShoppingListModel();
        public IngredientModel CurrentIngredientModel { get; set; } = new IngredientModel();

        [Inject]
        public IShoppingListService ShoppingListService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);

            if (id == 0)
            {
                Model = new ShoppingListModel();
            }
            else
            {
                Model = await ShoppingListService.Get(int.Parse(Id));
            }
        }

        protected async Task NavigateToOverview()
        {
            NavigationManager.NavigateTo($"/mealplanedit/{Id}");
        }
    }
}
