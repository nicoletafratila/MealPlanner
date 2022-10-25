namespace MealPlanner.UI.Web.Configs
{
    public class MealPlannerApiConfig : IApiConfig
    {
        private const string NAME = "MealPlanner";

        public Uri BaseUrl { get; set; }
        public int Timeout { get; set; }
        public string Name
        {
            get { return NAME; }
        }

        public MealPlannerApiConfig(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            configuration.Bind(NAME, this);
        }
    }
}
