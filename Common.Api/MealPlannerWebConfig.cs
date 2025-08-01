using Common.Constants;
using Microsoft.Extensions.Configuration;

namespace Common.Api
{
    public class MealPlannerWebConfig : ApiConfig
    {
        public override string? Name => ApiConfigNames.MealPlannerWeb;

        public MealPlannerWebConfig(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
