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


        //protected async Task SaveAsync()
        //{
        //    if (Recipe!.Id == 0)
        //    {
        //        var addedEntity = await RecipeService!.AddAsync(Recipe);
        //        if (addedEntity != null)
        //        {
        //            NavigateToOverview();
        //        }
        //    }
        //    else
        //    {
        //        await RecipeService!.UpdateAsync(Recipe);
        //        NavigateToOverview();
        //    }
        //}

        //private void CheckboxChanged(ProductModel productModel, object value)
        //{
        //    if (productModel != null)
        //    {
        //        var item = Model!.Products!.FirstOrDefault(i => i.Id == productModel.Id);
        //        if (item != null)
        //        {
        //            item.Collected = (bool)value;
        //        }
        //    }
        //}
    }
}
