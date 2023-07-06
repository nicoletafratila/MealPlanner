﻿using Common.Data.Entities;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ShoppingListEdit
    {
        [Parameter]
        public string? Id { get; set; }
        public ShoppingListModel? Model { get; set; }

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
            NavigationManager!.NavigateTo($"/shoppinglistoverview");
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