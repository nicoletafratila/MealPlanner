using Common.Constants;

namespace Common.Api
{
    public class MealPlannerApiConfig : ApiConfig
    {
        public override string Name => ApiConfigNames.MealPlanner;

        public MealPlannerApiConfig(IConfiguration configuration) : base(configuration)
        {
            Endpoints = new Dictionary<string, string>();
            Endpoints[ApiEndpointNames.ShoppingListApi] = "api/shoppinglist";
            Endpoints[ApiEndpointNames.MealPlanApi] = "api/mealplan";
        }
    }
}
