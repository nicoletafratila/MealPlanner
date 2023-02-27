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

        public async Task<IList<RecipeModel>> GetAll()
        {
            return await _httpClient.GetFromJsonAsync<IList<RecipeModel>>($"{ApiNames.RecipeApi}");
        }

        public async Task<RecipeModel> Get(int id)
        {
            return await _httpClient.GetFromJsonAsync<RecipeModel>($"{ApiNames.RecipeApi}/{id}");
        }

        public async Task<EditRecipeModel> GetEdit(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditRecipeModel>($"{ApiNames.RecipeApi}/edit/{id}");
        }

        public async Task<IList<RecipeModel>> Search(int categoryId)
        {
            return await _httpClient.GetFromJsonAsync<IList<RecipeModel>>($"{ApiNames.RecipeApi}/category/{categoryId}");
        }

        public async Task<EditRecipeModel> Add(EditRecipeModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(ApiNames.RecipeApi, modelJson);

            if (response.IsSuccessStatusCode)
            {
                return await JsonSerializer.DeserializeAsync<EditRecipeModel>(await response.Content.ReadAsStreamAsync());
            }

            return null;
        }

        public async Task Update(EditRecipeModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            await _httpClient.PutAsync(ApiNames.RecipeApi, modelJson);
        }

        public async Task DeleteAsync(int id)
        {
            await _httpClient.DeleteAsync($"{ApiNames.RecipeApi}/{id}");
        }
    }
}