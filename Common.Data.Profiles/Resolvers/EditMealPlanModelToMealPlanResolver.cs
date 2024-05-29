using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class EditMealPlanModelToMealPlanResolver : IMemberValueResolver<MealPlanEditModel, MealPlan, IList<RecipeModel>?, IList<MealPlanRecipe>?>
    {
        public IList<MealPlanRecipe>? Resolve(MealPlanEditModel source, MealPlan destination, IList<RecipeModel>? sourceValue, IList<MealPlanRecipe>? destValue, ResolutionContext context)
        {
            var result = new List<MealPlanRecipe>();
            if (source.Recipes != null && source.Recipes.Any())
            {
                foreach (var item in source.Recipes)
                {
                    var mealPlanRecipe = new MealPlanRecipe
                    {
                        RecipeId = item.Id,
                        MealPlanId = source.Id
                    };
                    result.Add(mealPlanRecipe);
                }
            }
            return result.ToList();
        }
    }
}
