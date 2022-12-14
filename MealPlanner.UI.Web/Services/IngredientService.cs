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

        public async Task<IEnumerable<IngredientModel>> GetAll()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<IngredientModel>>($"{ApiNames.IngredientApi}");
        }

        public async Task<EditIngredientModel> Get(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditIngredientModel>($"{ApiNames.IngredientApi}{id}");
        }

        public async Task<EditIngredientModel> Add(EditIngredientModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(ApiNames.IngredientApi, modelJson);

            if (response.IsSuccessStatusCode)
            {
                return await JsonSerializer.DeserializeAsync<EditIngredientModel>(await response.Content.ReadAsStreamAsync());
            }

            return null;
        }

        public async Task Update(EditIngredientModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            await _httpClient.PutAsync(ApiNames.IngredientApi, modelJson);
        }
    }
}
