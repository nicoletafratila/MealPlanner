namespace MealPlanner.UI.Web.Configs
{
    public class RecipeBookApiConfig : Common.Api.ApiConfig
    {
        public override string Name => Common.Constants.ApiConfig.RecipeBook;

        public RecipeBookApiConfig(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
