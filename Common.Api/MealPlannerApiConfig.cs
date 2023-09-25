using Common.Constants;

namespace Common.Api
{
    public class MealPlannerApiConfig : ApiConfig
    {
        public override string Name => ApiConfigNames.MealPlanner;

        public MealPlannerApiConfig(IConfiguration configuration) : base(configuration)
        {
            Endpoints = new Dictionary<string, string>();
            Endpoints[ApiEndPointNames.ShoppingListApi] = "api/shoppinglist";
            Endpoints[ApiEndPointNames.MealPlanApi] = "api/mealplan";
        }
    }
}
