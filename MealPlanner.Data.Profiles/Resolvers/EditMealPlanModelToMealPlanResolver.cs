using AutoMapper;
using MealPlanner.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace MealPlanner.Data.Profiles.Resolvers
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
                    Id = Guid.NewGuid(),
                    RecipeId = r.Id,
                    MealPlanId = source.Id
                })
                .ToList();
        }
    }
}
