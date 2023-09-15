using Common.Constants;
using RecipeBook.Shared.Models;
using System.Text;
using System.Text.Json;

namespace MealPlanner.UI.Web.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly HttpClient _httpClient;

        public RecipeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IList<RecipeModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<RecipeModel>?>($"{ApiNames.RecipeApi}");
        }

        public async Task<RecipeModel?> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<RecipeModel?>($"{ApiNames.RecipeApi}/{id}");
        }

        public async Task<EditRecipeModel?> GetEditAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditRecipeModel?>($"{ApiNames.RecipeApi}/edit/{id}");
        }

        public async Task<IList<RecipeModel>?> SearchAsync(int categoryId)
        {
            return await _httpClient.GetFromJsonAsync<IList<RecipeModel>?>($"{ApiNames.RecipeApi}/search/{categoryId}");
        }

        public async Task<EditRecipeModel?> AddAsync(EditRecipeModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(ApiNames.RecipeApi, modelJson);

            if (response.IsSuccessStatusCode)
            {
                return await JsonSerializer.DeserializeAsync<EditRecipeModel?>(await response.Content.ReadAsStreamAsync());
            }

            return null;
        }

        public async Task UpdateAsync(EditRecipeModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            await _httpClient.PutAsync(ApiNames.RecipeApi, modelJson);
        }

        public async Task<string> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{ApiNames.RecipeApi}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return string.Empty;
        }
    }
}