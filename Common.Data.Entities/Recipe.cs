﻿namespace Common.Data.Entities
{
    public class Recipe : Entity<int>
    {
        public string Name { get; set; }
        public byte[]? ImageContent { get; set; }
        public IEnumerable<RecipeIngredient>? RecipeIngredients { get; set; }
        public IEnumerable<MealPlanRecipe>? MealPlanRecipes { get; set; }
    }
}