using AutoMapper;
using Common.Data.Entities;
using Common.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class RecipeToEditRecipeModelResolver : IMemberValueResolver<Recipe, RecipeEditModel, IList<RecipeIngredient>?, IList<RecipeIngredientEditModel>?>
    {
        public IList<RecipeIngredientEditModel>? Resolve(Recipe source, RecipeEditModel destination, IList<RecipeIngredient>? sourceValue, IList<RecipeIngredientEditModel>? destValue, ResolutionContext context)
        {
            var results = source.RecipeIngredients?.Select(context.Mapper.Map<RecipeIngredientEditModel>)
                         .OrderBy(item => item.Product?.ProductCategory?.Name)
                         .ThenBy(item => item.Product?.Name).ToList();
            results.SetIndexes();
            return results;
        }
    }
}
