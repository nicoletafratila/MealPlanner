using Common.Constants;
using RecipeBook.Shared.Models;
using System.Text.Json;
using System.Text;

namespace MealPlanner.UI.Web.Services
{
    public class IngredientService : IIngredientService
    {
        private readonly HttpClient _httpClient;

        public IngredientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IList<IngredientModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<IngredientModel>?>($"{ApiNames.IngredientApi}");
        }

        public async Task<EditIngredientModel?> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditIngredientModel?>($"{ApiNames.IngredientApi}/{id}");
        }

        public async Task<IList<IngredientModel>?> SearchAsync(int categoryId)
        {
            return await _httpClient.GetFromJsonAsync<IList<IngredientModel>?>($"{ApiNames.IngredientApi}/category/{categoryId}");
        }

        public async Task<EditIngredientModel?> AddAsync(EditIngredientModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(ApiNames.IngredientApi, modelJson);

            if (response.IsSuccessStatusCode)
            {
                return await JsonSerializer.DeserializeAsync<EditIngredientModel?>(await response.Content.ReadAsStreamAsync());
            }

            return null;
        }

        public async Task UpdateAsync(EditIngredientModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            await _httpClient.PutAsync(ApiNames.IngredientApi, modelJson);
        }

        public async Task DeleteAsync(int id)
        {
            await _httpClient.DeleteAsync($"{ApiNames.IngredientApi}/{id}");
        }
    }
}
