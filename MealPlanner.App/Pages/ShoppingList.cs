using MealPlanner.App.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.App.Pages
{
    public partial class ShoppingList
    {
         [Inject]
        public IShoppingListService ShoppingListService { get; set; }

        [Parameter]
        public string Id { get; set; }

        public ShoppingListModel Model { get; set; } = new ShoppingListModel();

        protected bool Saved;

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);
            Saved = false;

            if (id == 0)
            {
                Model = new ShoppingListModel();
            }
            else
            {
                Model = await ShoppingListService.Get(int.Parse(Id));
            }
        }

        protected async Task Save()
        {
            Saved = false;
        }
    }
}
