using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class EditRecipeModelToRecipeResolver : IMemberValueResolver<EditRecipeModel, Recipe, IList<RecipeIngredientModel>?, IList<RecipeIngredient>?>
    {
        public IList<RecipeIngredient>? Resolve(EditRecipeModel source, Recipe destination, IList<RecipeIngredientModel>? sourceValue, IList<RecipeIngredient>? destValue, ResolutionContext context)
        {
            return source.Ingredients!.Select(context.Mapper.Map<RecipeIngredient>).ToList();
        }
    }
}
