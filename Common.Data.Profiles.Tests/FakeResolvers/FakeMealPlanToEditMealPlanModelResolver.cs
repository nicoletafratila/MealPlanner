using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests.FakeResolvers
{
    public class FakeMealPlanToEditMealPlanModelResolver 
        : IMemberValueResolver<
            MealPlan,
            MealPlanEditModel,
            IList<MealPlanRecipe>?,
            IList<RecipeModel>?>
    {
        public bool WasCalled { get; private set; }
        public IList<RecipeModel>? ReturnedValue { get; set; }

        public IList<RecipeModel>? Resolve(
            MealPlan source,
            MealPlanEditModel destination,
            IList<MealPlanRecipe>? sourceValue,
            IList<RecipeModel>? destValue,
            ResolutionContext context)
        {
            WasCalled = true;
            return ReturnedValue;
        }
    }
}