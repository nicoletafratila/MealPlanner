using RecipeBook.Shared.Models;
using System.Text;
using System.Text.Json;

namespace MealPlanner.App.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly HttpClient _httpClient;

        public RecipeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<RecipeModel>> GetAll()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<RecipeModel>>($"api/recipe");
        }

        public async Task<EditRecipeModel> Get(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditRecipeModel>($"api/recipe/{id}");
        }

        public async Task<EditRecipeModel> Add(EditRecipeModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/recipe", modelJson);

            if (response.IsSuccessStatusCode)
            {
                return await JsonSerializer.DeserializeAsync<EditRecipeModel>(await response.Content.ReadAsStreamAsync());
            }

            return null;
        }

        public async Task Update(EditRecipeModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            await _httpClient.PutAsync("api/recipe", modelJson);
        }
    }
}