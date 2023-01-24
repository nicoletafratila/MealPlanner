using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class EditRecipeModelToRecipeResolver : IMemberValueResolver<EditRecipeModel, Recipe, IList<RecipeIngredientModel>, IList<RecipeIngredient>>
    {
        public IList<RecipeIngredient> Resolve(EditRecipeModel source, Recipe destination, IList<RecipeIngredientModel> sourceValue, IList<RecipeIngredient> destValue, ResolutionContext context)
        {
            var result = new List<RecipeIngredient>();
            if (source.Ingredients != null && source.Ingredients.Any())
            {
                foreach (var item in source.Ingredients)
                {
                    result.Add(context.Mapper.Map<RecipeIngredient>(item));
                }
            }
            return result;
        }
    }
}
