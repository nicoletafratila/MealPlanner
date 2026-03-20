using AutoMapper;
using Common.Data.Entities;
using Common.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class RecipeToEditRecipeModelResolver(IMapper mapper)
        : IMemberValueResolver<
            Recipe,
            RecipeEditModel,
            IList<RecipeIngredient>?,
            IList<RecipeIngredientEditModel>?>
    {
        public IList<RecipeIngredientEditModel>? Resolve(
            Recipe source,
            RecipeEditModel destination,
            IList<RecipeIngredient>? sourceValue,
            IList<RecipeIngredientEditModel>? destValue,
            ResolutionContext context)
        {
            if (sourceValue == null || sourceValue.Count == 0)
                return [];

            var results = sourceValue
                .Select(i => mapper.Map<RecipeIngredientEditModel>(i))
                .OrderBy(i => i.Product?.ProductCategory?.Name ?? string.Empty)
                .ThenBy(i => i.Product?.Name ?? string.Empty)
                .ToList();

            results.SetIndexes();
            return results;
        }
    }
}

