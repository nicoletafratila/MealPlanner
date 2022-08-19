﻿using AutoMapper;
using RecipeBook.Api.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Data.Profiles
{
    public class MealPlanIngredientCustomResolver : IMemberValueResolver<MealPlan, ShoppingListModel, IEnumerable<MealPlanRecipe>, IEnumerable<IngredientModel>>
    {
        public IEnumerable<IngredientModel> Resolve(MealPlan source, ShoppingListModel destination, IEnumerable<MealPlanRecipe> sourceValue, IEnumerable<IngredientModel> destValue, ResolutionContext context)
        {
            var result = new List<IngredientModel>();
            foreach (var item in source.MealPlanRecipes)
            {
                result.AddRange(context.Mapper.Map<EditRecipeModel>(item.Recipe).Ingredients);
            }
            return result;
        }
    }
}
