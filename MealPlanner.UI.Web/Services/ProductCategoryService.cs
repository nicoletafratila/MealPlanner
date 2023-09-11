using Common.Constants;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly HttpClient _httpClient;

        public ProductCategoryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IList<ProductCategoryModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<ProductCategoryModel>>($"{ApiNames.IngredientCategoryApi}");
        }
    }
}
