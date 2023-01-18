using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class MealPlanIngredientCustomResolver : IMemberValueResolver<MealPlan, ShoppingListModel, IList<MealPlanRecipe>, IList<RecipeIngredientModel>>
    {
        public IList<RecipeIngredientModel> Resolve(MealPlan source, ShoppingListModel destination, IList<MealPlanRecipe> sourceValue, IList<RecipeIngredientModel> destValue, ResolutionContext context)
        {
            var result = new List<RecipeIngredientModel>();
            foreach (var item in source.MealPlanRecipes)
            {
                result.AddRange(context.Mapper.Map<EditRecipeModel>(item.Recipe).Ingredients);
            }
            return result.OrderBy(item => item.Ingredient.IngredientCategory.DisplaySequence).ThenBy(item => item.Ingredient.Name).ToList();
        }
    }
}
