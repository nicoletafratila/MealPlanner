using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class EditRecipeModelToRecipeResolver : IMemberValueResolver<RecipeEditModel, Recipe, IList<RecipeIngredientEditModel>?, IList<RecipeIngredient>?>
    {
        public IList<RecipeIngredient>? Resolve(RecipeEditModel source, Recipe destination, IList<RecipeIngredientEditModel>? sourceValue, IList<RecipeIngredient>? destValue, ResolutionContext context)
        {
            return source.Ingredients?.Select(context.Mapper.Map<RecipeIngredient>).ToList();
        }
    }
}
