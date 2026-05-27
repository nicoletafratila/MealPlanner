using Common.Api;
using Common.Constants;

namespace MealPlanner.Api.Abstractions
{
    public class RecipeBookClientConfig : ApiConfig
    {
        public override string? Name => ApiConfigNames.RecipeBook;

        public RecipeBookClientConfig(IConfiguration configuration) : base(configuration)
        {
            Controllers = new Dictionary<string, string>
            {
                [RecipeBookControllers.RecipeCategory] = "api/recipecategory",
                [RecipeBookControllers.ProductCategory] = "api/productcategory"
            };
        }
    }
}
