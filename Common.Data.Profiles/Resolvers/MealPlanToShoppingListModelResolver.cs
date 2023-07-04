using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class MealPlanToShoppingListModelResolver : IMemberValueResolver<MealPlan, ShoppingListModel, IList<MealPlanRecipe>?, IList<ProductModel>?>
    {
        public IList<ProductModel>? Resolve(MealPlan source, ShoppingListModel destination, IList<MealPlanRecipe>? sourceValue, IList<ProductModel>? destValue, ResolutionContext context)
        {
            if (source.MealPlanRecipes is null || !source.MealPlanRecipes.Any())
                return new List<ProductModel>();

            var products = new List<RecipeIngredientModel>();
            foreach (var item in source.MealPlanRecipes)
            {
                var recipeIngredients = context.Mapper.Map<EditRecipeModel>(item.Recipe).Ingredients;
                if (recipeIngredients != null)
                {
                    foreach (var i in recipeIngredients)
                    {
                        var existingIngredient = products.FirstOrDefault(x => x.Ingredient!.Id == i.Ingredient!.Id);
                        if (existingIngredient == null)
                            products.Add(i);
                        else
                            existingIngredient.Quantity += i.Quantity;
                    }
                }
            }

            return products.OrderBy(i => i.Ingredient!.IngredientCategory!.DisplaySequence)
                           .Select(i => context.Mapper.Map<ProductModel>(i))
                           .ToList();
        }
    }
}
