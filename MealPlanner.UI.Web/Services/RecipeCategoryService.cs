using Common.Constants;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class RecipeCategoryService : IRecipeCategoryService
    {
        private readonly HttpClient _httpClient;

        public RecipeCategoryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IList<RecipeCategoryModel>> GetAll()
        {
            return await _httpClient.GetFromJsonAsync<IList<RecipeCategoryModel>>($"{ApiNames.RecipeCategoryApi}");
        }
    }
}
