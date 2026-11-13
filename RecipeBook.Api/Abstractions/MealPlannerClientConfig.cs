using Common.Core;
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
                [ShopRoutes.ControllerName] = "api/shop",
                [MealPlanRoutes.ControllerName] = "api/mealplan"
            };
        }
    }
}
