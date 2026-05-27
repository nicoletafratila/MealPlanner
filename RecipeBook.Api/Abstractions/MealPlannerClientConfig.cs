using Common.Api;
using MealPlanner.Shared.Constants;

namespace RecipeBook.Api.Abstractions
{
    public class MealPlannerClientConfig : ApiConfig
    {
        public override string? Name => ApiConfigNames.MealPlanner;

        public MealPlannerClientConfig(IConfiguration configuration) : base(configuration)
        {
            Controllers = new Dictionary<string, string>
            {
                [MealPlannerControllers.Shop] = "api/shop",
                [MealPlannerControllers.MealPlan] = "api/mealplan"
            };
        }
    }
}
