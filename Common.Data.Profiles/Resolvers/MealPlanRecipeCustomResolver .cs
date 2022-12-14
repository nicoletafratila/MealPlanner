using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class MealPlanRecipeCustomResolver : IMemberValueResolver<MealPlan, EditMealPlanModel, IEnumerable<MealPlanRecipe>, IEnumerable<RecipeModel>>
    {
        public IEnumerable<RecipeModel> Resolve(MealPlan source, EditMealPlanModel destination, IEnumerable<MealPlanRecipe> sourceValue, IEnumerable<RecipeModel> destValue, ResolutionContext context)
        {
            return source.MealPlanRecipes.Select(item => context.Mapper.Map<RecipeModel>(item.Recipe)).OrderBy(item => item.RecipeCategory.DisplaySequence).ThenBy(item => item.Name);
        }
    }
}
