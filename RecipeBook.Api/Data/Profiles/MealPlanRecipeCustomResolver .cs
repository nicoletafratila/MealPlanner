using AutoMapper;
using RecipeBook.Api.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Data.Profiles
{
    public class MealPlanRecipeCustomResolver : IMemberValueResolver<MealPlan, EditMealPlanModel, IEnumerable<MealPlanRecipe>, IEnumerable<RecipeModel>>
    {
        public IEnumerable<RecipeModel> Resolve(MealPlan source, EditMealPlanModel destination, IEnumerable<MealPlanRecipe> sourceValue, IEnumerable<RecipeModel> destValue, ResolutionContext context)
        {
            return source.MealPlanRecipes.Select(item => context.Mapper.Map<RecipeModel>(item.Recipe));
        }
    }
}
