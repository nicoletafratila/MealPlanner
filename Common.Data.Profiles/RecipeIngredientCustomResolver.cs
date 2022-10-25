using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles
{
    public class RecipeIngredientCustomResolver : IMemberValueResolver<Recipe, EditRecipeModel, IEnumerable<RecipeIngredient>, IEnumerable<IngredientModel>>
    {
        public IEnumerable<IngredientModel> Resolve(Recipe source, EditRecipeModel destination, IEnumerable<RecipeIngredient> sourceValue, IEnumerable<IngredientModel> destValue, ResolutionContext context)
        {
            var result = new List<IngredientModel>();
            if (source.RecipeIngredients.Any())
            {
                foreach (var item in source.RecipeIngredients)
                {
                    var model = context.Mapper.Map<IngredientModel>(item.Ingredient);
                    model.RecipeId = item.RecipeId;
                    model.Quantity = item.Quantity;
                    result.Add(model);
                }
            }
            return result;
        }
    }
}
