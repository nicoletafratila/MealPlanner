using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests.FakeResolvers
{
    public class FakeRecipeToEditRecipeModelResolver
        : IMemberValueResolver<
            Recipe,
            RecipeEditModel,
            IList<RecipeIngredient>?,
            IList<RecipeIngredientEditModel>?>
    {
        public bool WasCalled { get; private set; }
        public IList<RecipeIngredientEditModel>? ReturnedValue { get; set; }

        public IList<RecipeIngredientEditModel>? Resolve(
            Recipe source,
            RecipeEditModel destination,
            IList<RecipeIngredient>? sourceValue,
            IList<RecipeIngredientEditModel>? destValue,
            ResolutionContext context)
        {
            WasCalled = true;
            return ReturnedValue;
        }
    }
}
