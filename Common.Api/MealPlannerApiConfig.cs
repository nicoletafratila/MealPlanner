using Common.Constants;
using Microsoft.Extensions.Configuration;

namespace Common.Api
{
    public class MealPlannerApiConfig : ApiConfig
    {
        public override string Name => ApiConfigNames.MealPlanner;

        public MealPlannerApiConfig(IConfiguration configuration) : base(configuration)
        {
            Endpoints = new Dictionary<string, string>
            {
                [ApiEndpointNames.ShoppingListApi] = "api/shoppinglist",
                [ApiEndpointNames.MealPlanApi] = "api/mealplan",
                [ApiEndpointNames.ShopApi] = "api/shop"
            };
        }
    }
}
