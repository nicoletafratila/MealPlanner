using Common.Constants;
using Microsoft.Extensions.Configuration;

namespace Common.Api
{
    public class MealPlannerApiConfig : ApiConfig
    {
        public override string? Name => ApiConfigNames.MealPlanner;

        public MealPlannerApiConfig(IConfiguration configuration) : base(configuration)
        {
            Controllers = new Dictionary<string, string>
            {
                [MealPlannerControllers.ShoppingList] = "api/shoppinglist",
                [MealPlannerControllers.MealPlan] = "api/mealplan",
                [MealPlannerControllers.Shop] = "api/shop",
                [MealPlannerControllers.Statistics] = "api/statistics"
            };
        }
    }
}
