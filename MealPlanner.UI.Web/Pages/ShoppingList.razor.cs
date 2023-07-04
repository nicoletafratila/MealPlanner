using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ShoppingList
    {
        [Parameter]
        public string? Id { get; set; }

        public ShoppingListModel? Model { get; set; }
        public ShoppingIngredientModel? IngredientModel { get; set; }

        [Inject]
        public IShoppingListService? ShoppingListService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);

            if (id == 0)
            {
                Model = new ShoppingListModel();
            }
            else
            {
                Model = await ShoppingListService!.GetByIdAsync(int.Parse(Id!));
            }
        }

        protected void NavigateToOverview()
        {
            NavigationManager!.NavigateTo($"/mealplanedit/{Id}");
        }

        private void CheckboxChanged(ShoppingIngredientModel ingredientModel, ChangeEventArgs e)
        {
            if (ingredientModel != null)
            {
                var item = Model!.Ingredients!.FirstOrDefault(i => i.Id == ingredientModel.Id);
                if (item != null)
                {
                    item.Collected = (bool)e.Value!;
                }
                Model!.Ingredients = Model!.Ingredients!.Where(i => !i.Collected).ToList();
                StateHasChanged();

                //if (UserService != null)
                //{
                //    await UserService.UpdateAsync(user);
                //}
                //await LoadDataAsync();
            }
        }
    }
}
