using Common.Constants;

namespace MealPlanner.UI.Web.Configs
{
    public class RecipeBookApiConfig : IApiConfig
    {
        public Uri BaseUrl { get; set; }
        public int Timeout { get; set; }
        public string Name => ApiConfig.RecipeBook;

        public RecipeBookApiConfig(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            configuration.Bind(Name, this);
        }
    }
}
