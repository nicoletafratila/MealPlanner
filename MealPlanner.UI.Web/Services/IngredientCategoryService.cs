using Common.Constants;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class IngredientCategoryService : IIngredientCategoryService
    {
        private readonly HttpClient _httpClient;

        public IngredientCategoryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<IngredientCategoryModel>> GetAll()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<IngredientCategoryModel>>($"{ApiNames.IngredientCategoryApi}");
        }
    }
}
