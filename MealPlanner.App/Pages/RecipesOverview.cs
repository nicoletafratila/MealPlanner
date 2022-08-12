﻿using MealPlanner.App.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.App.Pages
{
    public partial class RecipesOverview
    {
        public IEnumerable<RecipeModel> Recipes { get; set; } = new List<RecipeModel>();

        [Inject]
        public IRecipeService RecipeDataService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Recipes = await RecipeDataService.GetAll();
        }
    }
}
