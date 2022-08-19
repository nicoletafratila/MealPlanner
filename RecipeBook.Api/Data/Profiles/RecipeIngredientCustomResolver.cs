using AutoMapper;
using RecipeBook.Api.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Data.Profiles
{
    public class RecipeIngredientCustomResolver : IMemberValueResolver<Recipe, EditRecipeModel, IEnumerable<RecipeIngredient>, IEnumerable<IngredientModel>>
    {
        public IEnumerable<IngredientModel> Resolve(Recipe source, EditRecipeModel destination, IEnumerable<RecipeIngredient> sourceValue, IEnumerable<IngredientModel> destValue, ResolutionContext context)
        {
            var result = new List<IngredientModel>();
            foreach (var item in source.RecipeIngredients)
            {
                var model = context.Mapper.Map<IngredientModel>(item.Ingredient);
                model.RecipeId = item.RecipeId;
                model.IngredientId = item.IngredientId;
                model.Quantity = item.Quantity;
                result.Add(model);
            }
            return result;
        }
    }
}
