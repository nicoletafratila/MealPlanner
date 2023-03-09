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

        public async Task<IList<IngredientCategoryModel>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<IngredientCategoryModel>>($"{ApiNames.IngredientCategoryApi}");
        }
    }
}
