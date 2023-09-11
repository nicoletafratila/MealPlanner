using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ShoppingListEdit
    {
        [Parameter]
        public string? Id { get; set; }
        public EditShoppingListModel? Model { get; set; }

        [Inject]
        public IShoppingListService? ShoppingListService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);
            if (id == 0)
            {
                Model = new EditShoppingListModel();
            }
            else
            {
                Model = await ShoppingListService!.GetEditAsync(int.Parse(Id!));
            }
        }

        protected void NavigateToOverview()
        {
            NavigationManager!.NavigateTo($"/shoppinglistsoverview");
        }

        private async void CheckboxChanged(ShoppingListProductModel model)
        {
            var itemToChange = Model!.Products!.FirstOrDefault(item => item.Product!.Id == model!.Product!.Id);
            if (itemToChange != null)
            {
                itemToChange.Collected = !itemToChange.Collected;
            }
            await ShoppingListService!.UpdateAsync(Model);
            await OnInitializedAsync();
            StateHasChanged();
        }
    }
}
