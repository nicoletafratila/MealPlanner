using AutoMapper;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    //public class EditMealPlanModelToShoppingListModelResolver : IMemberValueResolver<EditMealPlanModel, EditShoppingListModel, IList<RecipeModel>?, IList<ShoppingListProductModel>?>
    //{
    //    public IList<ShoppingListProductModel>? Resolve(EditMealPlanModel source, EditShoppingListModel destination, IList<RecipeModel>? sourceValue, IList<ShoppingListProductModel>? destValue, ResolutionContext context)
    //    {
    //        if (source.Recipes is null || !source.Recipes.Any())
    //            return new List<ShoppingListProductModel>();

    //        var products = new List<RecipeIngredientModel>();
    //        foreach (var item in source.Recipes)
    //        {
    //            var recipeIngredients = context.Mapper.Map<EditRecipeModel>(item).Ingredients;
    //            if (recipeIngredients != null)
    //            {
    //                foreach (var i in recipeIngredients)
    //                {
    //                    var existingIngredient = products.FirstOrDefault(x => x.Product!.Id == i.Product!.Id);
    //                    if (existingIngredient == null)
    //                        products.Add(i);
    //                    else
    //                        existingIngredient.Quantity += i.Quantity;
    //                }
    //            }
    //        }

    //        return products.OrderBy(i => i.Product!.IngredientCategory!.DisplaySequence)
    //                       .Select(i => context.Mapper.Map<ShoppingListProductModel>(i))
    //                       .ToList();
    //    }
    //}
}
