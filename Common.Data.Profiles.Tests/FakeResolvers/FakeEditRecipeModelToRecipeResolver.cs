using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests.FakeResolvers
{
    public class FakeEditRecipeModelToRecipeResolver
         : IMemberValueResolver<
            RecipeEditModel,
            Recipe,
            IList<RecipeIngredientEditModel>?,
            IList<RecipeIngredient>?>
    {
        public bool WasCalled { get; private set; }
        public IList<RecipeIngredient>? ReturnedValue { get; set; }

        public IList<RecipeIngredient>? Resolve(
            RecipeEditModel source,
            Recipe destination,
            IList<RecipeIngredientEditModel>? sourceValue,
            IList<RecipeIngredient>? destValue,
            ResolutionContext context)
        {
            WasCalled = true;
            return ReturnedValue;
        }
    }

}
