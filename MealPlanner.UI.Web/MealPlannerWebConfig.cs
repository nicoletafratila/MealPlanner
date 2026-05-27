using Common.Api;
using Common.Constants;

namespace MealPlanner.UI.Web
{
    public class MealPlannerWebConfig : ApiConfig
    {
        public override string? Name => ApiConfigNames.MealPlannerWeb;

        public MealPlannerWebConfig(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
