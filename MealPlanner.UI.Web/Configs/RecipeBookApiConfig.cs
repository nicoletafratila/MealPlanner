namespace MealPlanner.UI.Web.Configs
{
    public class RecipeBookApiConfig : IApiConfig
    {
        private const string NAME = "RecipeBook";

        public Uri BaseUrl { get; set; }
        public int Timeout { get; set; }
        public string Name
        {
            get { return NAME; }
        }

        public RecipeBookApiConfig(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            configuration.Bind(NAME, this);
        }
    }
}
