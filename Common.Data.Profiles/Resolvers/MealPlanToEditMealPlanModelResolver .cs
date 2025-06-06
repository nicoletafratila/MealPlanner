﻿using AutoMapper;
using Common.Data.Entities;
using Common.Models;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class MealPlanToEditMealPlanModelResolver : IMemberValueResolver<MealPlan, MealPlanEditModel, IList<MealPlanRecipe>?, IList<RecipeModel>?>
    {
        public IList<RecipeModel>? Resolve(MealPlan source, MealPlanEditModel destination, IList<MealPlanRecipe>? sourceValue, IList<RecipeModel>? destValue, ResolutionContext context)
        {
            var results = source.MealPlanRecipes?.Select(item => context.Mapper.Map<RecipeModel>(item.Recipe))
                                          .OrderBy(item => item.RecipeCategory?.DisplaySequence)
                                          .ThenBy(item => item.Name).ToList();
            results!.SetIndexes();
            return results;
        }
    }
}
