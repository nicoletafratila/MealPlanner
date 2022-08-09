namespace MealPlanner.App.Services
{
    public class RecipeBookApiConfig : IRecipeBookApiConfig
    {
        public Uri BaseUrl { get; set; }
        public int Timeout { get; set; }
        
        public RecipeBookApiConfig(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            configuration.Bind("RecipeBook", this);
        }
    }
}
