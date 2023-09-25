using Common.Api;
using Common.Constants;
using RecipeBook.Shared.Models;
using System.Text;
using System.Text.Json;

namespace MealPlanner.UI.Web.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly HttpClient _httpClient;
        private readonly IApiConfig _recipeBookApiConfig;

        public RecipeService(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _recipeBookApiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.RecipeBook);
        }

        public async Task<IList<RecipeModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<RecipeModel>?>(_recipeBookApiConfig.Endpoints[ApiEndpointNames.RecipeApi]);
        }

        public async Task<RecipeModel?> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<RecipeModel?>($"{_recipeBookApiConfig.Endpoints[ApiEndpointNames.RecipeApi]}/{id}");
        }

        public async Task<EditRecipeModel?> GetEditAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditRecipeModel?>($"{_recipeBookApiConfig.Endpoints[ApiEndpointNames.RecipeApi]}/edit/{id}");
        }

        public async Task<IList<RecipeModel>?> SearchAsync(int categoryId)
        {
            return await _httpClient.GetFromJsonAsync<IList<RecipeModel>?>($"{_recipeBookApiConfig.Endpoints[ApiEndpointNames.RecipeApi]}/search/{categoryId}");
        }

        public async Task<EditRecipeModel?> AddAsync(EditRecipeModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_recipeBookApiConfig.Endpoints[ApiEndpointNames.RecipeApi], modelJson);

            if (response.IsSuccessStatusCode)
            {
                return await JsonSerializer.DeserializeAsync<EditRecipeModel?>(await response.Content.ReadAsStreamAsync());
            }

            return null;
        }

        public async Task UpdateAsync(EditRecipeModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            await _httpClient.PutAsync(_recipeBookApiConfig.Endpoints[ApiEndpointNames.RecipeApi], modelJson);
        }

        public async Task<string> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_recipeBookApiConfig.Endpoints[ApiEndpointNames.RecipeApi]}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return string.Empty;
        }
    }
}