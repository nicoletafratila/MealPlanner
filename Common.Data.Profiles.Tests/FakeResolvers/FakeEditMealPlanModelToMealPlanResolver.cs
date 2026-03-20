using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests.FakeResolvers
{
    public class FakeEditMealPlanModelToMealPlanResolver
        : IMemberValueResolver<
            MealPlanEditModel,
            MealPlan,
            IList<RecipeModel>?,
            IList<MealPlanRecipe>?
        >
    {
        public bool WasCalled { get; private set; }
        public IList<MealPlanRecipe>? ReturnedValue { get; set; }

        public IList<MealPlanRecipe>? Resolve(
            MealPlanEditModel source,
            MealPlan destination,
            IList<RecipeModel>? sourceValue,
            IList<MealPlanRecipe>? destValue,
            ResolutionContext context)
        {
            WasCalled = true;
            return ReturnedValue;
        }
    }
}
