using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class RecipeToEditRecipeModelResolver : IMemberValueResolver<Recipe, EditRecipeModel, IList<RecipeIngredient>?, IList<EditRecipeIngredientModel>?>
    {
        public IList<EditRecipeIngredientModel>? Resolve(Recipe source, EditRecipeModel destination, IList<RecipeIngredient>? sourceValue, IList<EditRecipeIngredientModel>? destValue, ResolutionContext context)
        {
            return source.RecipeIngredients?.Select(context.Mapper.Map<EditRecipeIngredientModel>)
                         .OrderBy(item => item.Product?.ProductCategory?.Name)
                         .ThenBy(item => item.Product?.Name).ToList();
        }
    }
}
