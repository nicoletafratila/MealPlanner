using AutoMapper;
using Common.Data.Entities;
using Common.Models;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class MealPlanToEditMealPlanModelResolver()
        : IMemberValueResolver<
            MealPlan,
            MealPlanEditModel,
            IList<MealPlanRecipe>?,
            IList<RecipeModel>?>
    {
        public IList<RecipeModel>? Resolve(
            MealPlan source,
            MealPlanEditModel destination,
            IList<MealPlanRecipe>? sourceValue,
            IList<RecipeModel>? destValue,
            ResolutionContext context)
        {
            if (sourceValue == null || sourceValue.Count == 0)
                return [];

            var results = sourceValue
                .Select(mpr => context.Mapper.Map<RecipeModel>(mpr.Recipe))
                .OrderBy(r => r.RecipeCategory?.DisplaySequence ?? int.MaxValue)
                .ThenBy(r => r.Name ?? string.Empty)
                .ToList();

            results.SetIndexes();
            return results;
        }
    }
}