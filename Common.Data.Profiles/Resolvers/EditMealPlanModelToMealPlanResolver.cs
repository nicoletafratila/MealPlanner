using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class EditMealPlanModelToMealPlanResolver
        : IMemberValueResolver<
            MealPlanEditModel,
            MealPlan,
            IList<RecipeModel>?,
            IList<MealPlanRecipe>?>
    {
        public IList<MealPlanRecipe>? Resolve(
            MealPlanEditModel source,
            MealPlan destination,
            IList<RecipeModel>? sourceValue,
            IList<MealPlanRecipe>? destValue,
            ResolutionContext context)
        {
            if (sourceValue == null || sourceValue.Count == 0)
                return [];

            return sourceValue
                .Select(r => new MealPlanRecipe
                {
                    RecipeId = r.Id,
                    MealPlanId = source.Id
                })
                .ToList();
        }
    }
}