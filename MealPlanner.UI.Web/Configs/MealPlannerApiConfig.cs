using Common.Api;
using Common.Constants;

namespace MealPlanner.UI.Web.Configs
{
    public class MealPlannerApiConfig : Common.Api.ApiConfig
    {
        public override string Name => Common.Constants.ApiConfig.MealPlanner;

        public MealPlannerApiConfig(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
