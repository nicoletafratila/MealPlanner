using RecipeBook.Api.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Data.Profiles
{
    public static class IngredientMapper
    {
        public static IngredientModel ToIngredientModel(this RecipeIngredient item)
        {
            if (item.Ingredient == null)
            {
                return null;
            }

            return new IngredientModel()
            {
                RecipeId = item.RecipeId,
                IngredientId = item.IngredientId,
                Name = item.Ingredient.Name.TrimEnd(),
                Unit = item.Ingredient.Unit.TrimEnd(),
                Quantity = item.Quantity
            };
        }
    }
}
