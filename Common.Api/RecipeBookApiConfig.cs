using Common.Constants;
using Microsoft.Extensions.Configuration;

namespace Common.Api
{
    public class RecipeBookApiConfig : ApiConfig
    {
        public override string? Name => ApiConfigNames.RecipeBook;

        public RecipeBookApiConfig(IConfiguration configuration) : base(configuration)
        {
            Endpoints = new Dictionary<string, string>
            {
                [ApiEndpointNames.RecipeApi] = "api/recipe",
                [ApiEndpointNames.RecipeCategoryApi] = "api/recipecategory",
                [ApiEndpointNames.ProductApi] = "api/product",
                [ApiEndpointNames.ProductCategoryApi] = "api/productcategory",
                [ApiEndpointNames.UnitApi] = "api/unit"
            };
        }
    }
}
