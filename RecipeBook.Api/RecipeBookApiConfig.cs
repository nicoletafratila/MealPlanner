using Common.Api;
using RecipeBook.Shared.Constants;

namespace RecipeBook.Api
{
    public class RecipeBookApiConfig : ApiConfig
    {
        public override string? Name => ApiConfigNames.RecipeBook;

        public RecipeBookApiConfig(IConfiguration configuration) : base(configuration)
        {
            Controllers = new Dictionary<string, string>
            {
                [RecipeBookControllers.Recipe] = "api/recipe",
                [RecipeBookControllers.RecipeCategory] = "api/recipecategory",
                [RecipeBookControllers.Product] = "api/product",
                [RecipeBookControllers.ProductCategory] = "api/productcategory",
                [RecipeBookControllers.Unit] = "api/unit"
            };
        }
    }
}
