using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class EditMealPlanModelToMealPlanResolver : IMemberValueResolver<EditMealPlanModel, MealPlan, IList<RecipeModel>?, IList<MealPlanRecipe>?>
    {
        public IList<MealPlanRecipe>? Resolve(EditMealPlanModel source, MealPlan destination, IList<RecipeModel>? sourceValue, IList<MealPlanRecipe>? destValue, ResolutionContext context)
        {
            var result = new List<MealPlanRecipe>();
            if (source.Recipes != null && source.Recipes.Any())
            {
                foreach (var item in source.Recipes)
                {
                    var mealPlanRecipe = new MealPlanRecipe();
                    mealPlanRecipe.RecipeId = item.Id;
                    mealPlanRecipe.MealPlanId = source.Id;
                    result.Add(mealPlanRecipe);
                }
            }
            return result.ToList();
        }
    }
}
