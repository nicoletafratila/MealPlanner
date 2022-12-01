using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class MealPlanIngredientCustomResolver : IMemberValueResolver<MealPlan, ShoppingListModel, IEnumerable<MealPlanRecipe>, IEnumerable<RecipeIngredientModel>>
    {
        public IEnumerable<RecipeIngredientModel> Resolve(MealPlan source, ShoppingListModel destination, IEnumerable<MealPlanRecipe> sourceValue, IEnumerable<RecipeIngredientModel> destValue, ResolutionContext context)
        {
            var result = new List<RecipeIngredientModel>();
            foreach (var item in source.MealPlanRecipes)
            {
                result.AddRange(context.Mapper.Map<EditRecipeModel>(item.Recipe).Ingredients);
            }
            return result.OrderBy(item => item.DisplaySequence).ThenBy(item => item.Name);
        }
    }
}
