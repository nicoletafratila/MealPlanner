using Common.Constants;
using RecipeBook.Shared.Models;
using System.Text.Json;
using System.Text;

namespace MealPlanner.UI.Web.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IList<ProductModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<ProductModel>?>($"{ApiNames.IngredientApi}");
        }

        public async Task<EditProductModel?> GetEditAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditProductModel?>($"{ApiNames.IngredientApi}/{id}");
        }

        public async Task<IList<ProductModel>?> SearchAsync(int categoryId)
        {
            return await _httpClient.GetFromJsonAsync<IList<ProductModel>?>($"{ApiNames.IngredientApi}/category/{categoryId}");
        }

        public async Task<EditProductModel?> AddAsync(EditProductModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(ApiNames.IngredientApi, modelJson);

            if (response.IsSuccessStatusCode)
            {
                return await JsonSerializer.DeserializeAsync<EditProductModel?>(await response.Content.ReadAsStreamAsync());
            }

            return null;
        }

        public async Task UpdateAsync(EditProductModel model)
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
