﻿using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class RecipeToEditRecipeModelResolver : IMemberValueResolver<Recipe, EditRecipeModel, IList<RecipeIngredient>, IList<RecipeIngredientModel>>
    {
        public IList<RecipeIngredientModel> Resolve(Recipe source, EditRecipeModel destination, IList<RecipeIngredient> sourceValue, IList<RecipeIngredientModel> destValue, ResolutionContext context)
        {
            var result = new List<RecipeIngredientModel>();
            if (source.RecipeIngredients != null && source.RecipeIngredients.Any())
            {
                foreach (var item in source.RecipeIngredients)
                {
                    result.Add(context.Mapper.Map<RecipeIngredientModel>(item));
                }
            }
            return result.OrderBy(item => item.Ingredient.IngredientCategory.DisplaySequence).ThenBy(item => item.Ingredient.Name).ToList();
        }
    }
}