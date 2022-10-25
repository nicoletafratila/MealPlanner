using Common.Constants;

namespace MealPlanner.UI.Web.Configs
{
    public class MealPlannerApiConfig : IApiConfig
    {
        public Uri BaseUrl { get; set; }
        public int Timeout { get; set; }
        public string Name => ApiConfig.MealPlanner;

        public MealPlannerApiConfig(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            configuration.Bind(Name, this);
        }
    }
}
