using Common.Constants;

namespace Common.Api
{
    public class RecipeBookApiConfig : ApiConfig
    {
        public override string Name => ApiConfigNames.RecipeBook;

        public RecipeBookApiConfig(IConfiguration configuration) : base(configuration)
        {
            Endpoints = new Dictionary<string, string>();
            Endpoints[ApiEndpointNames.RecipeApi] = "api/recipe";
            Endpoints[ApiEndpointNames.RecipeCategoryApi] = "api/recipecategory";
            Endpoints[ApiEndpointNames.ProductApi] = "api/product";
            Endpoints[ApiEndpointNames.ProductCategoryApi] = "api/productcategory";
            Endpoints[ApiEndpointNames.UnitApi] = "api/unit";
        }
    }
}
