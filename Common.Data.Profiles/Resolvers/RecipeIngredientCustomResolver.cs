using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class RecipeIngredientCustomResolver : IMemberValueResolver<Recipe, EditRecipeModel, IEnumerable<RecipeIngredient>, IEnumerable<RecipeIngredientModel>>
    {
        public IEnumerable<RecipeIngredientModel> Resolve(Recipe source, EditRecipeModel destination, IEnumerable<RecipeIngredient> sourceValue, IEnumerable<RecipeIngredientModel> destValue, ResolutionContext context)
        {
            var result = new List<RecipeIngredientModel>();
            if (source.RecipeIngredients != null && source.RecipeIngredients.Any())
            {
                foreach (var item in source.RecipeIngredients)
                {
                    var model = context.Mapper.Map<RecipeIngredientModel>(item.Ingredient);
                    model.RecipeId = item.RecipeId;
                    model.Quantity = item.Quantity;
                    model.Category = item.Ingredient.IngredientCategory.Name;
                    model.DisplaySequence = item.Ingredient.IngredientCategory.DisplaySequence;
                    result.Add(model);
                }
            }
            return result.OrderBy(item => item.DisplaySequence).ThenBy(item => item.Name);
        }
    }
}
