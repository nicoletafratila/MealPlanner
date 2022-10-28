using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
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
                    model.Category = item.Ingredient.Category.Name;
                    model.DisplaySequence = item.Ingredient.Category.DisplaySequence;
                    result.Add(model);
                }
            }
            return result.OrderBy(item => item.DisplaySequence);
        }
    }
}
