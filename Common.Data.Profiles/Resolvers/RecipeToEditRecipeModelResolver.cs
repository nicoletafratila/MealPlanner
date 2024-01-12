using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class RecipeToEditRecipeModelResolver : IMemberValueResolver<Recipe, EditRecipeModel, IList<RecipeIngredient>?, IList<RecipeIngredientModel>?>
    {
        public IList<RecipeIngredientModel>? Resolve(Recipe source, EditRecipeModel destination, IList<RecipeIngredient>? sourceValue, IList<RecipeIngredientModel>? destValue, ResolutionContext context)
        {
            return source.RecipeIngredients!.Select(context.Mapper.Map<RecipeIngredientModel>)
                         .OrderBy(item => item.Product!.ProductCategory!.Name)
                         .ThenBy(item => item.Product!.Name).ToList();
        }
    }
}
