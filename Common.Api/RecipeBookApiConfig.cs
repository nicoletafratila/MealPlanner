using Common.Constants;

namespace Common.Api
{
    public class RecipeBookApiConfig : ApiConfig
    {
        public override string Name => ApiConfigNames.RecipeBook;

        public RecipeBookApiConfig(IConfiguration configuration) : base(configuration)
        {
            Endpoints = new Dictionary<string, string>();
            Endpoints[ApiEndPointNames.RecipeApi] = "api/recipe";
            Endpoints[ApiEndPointNames.RecipeCategoryApi] = "api/recipecategory";
            Endpoints[ApiEndPointNames.ProductApi] = "api/product";
            Endpoints[ApiEndPointNames.ProductCategoryApi] = "api/productcategory";
            Endpoints[ApiEndPointNames.UnitApi] = "api/unit";
        }
    }
}
