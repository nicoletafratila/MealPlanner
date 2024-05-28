using System.Text;
using System.Text.Json;
using Common.Api;
using Common.Constants;
using Common.Pagination;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class RecipeCategoryService(HttpClient httpClient, IServiceProvider serviceProvider) : IRecipeCategoryService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IApiConfig _apiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.RecipeBook);

        public async Task<EditRecipeCategoryModel?> GetEditAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditRecipeCategoryModel?>($"{_apiConfig?.Endpoints![ApiEndpointNames.RecipeCategoryApi]}/edit/{id}");
        }

        public async Task<IList<RecipeCategoryModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<RecipeCategoryModel>?>(_apiConfig?.Endpoints![ApiEndpointNames.RecipeCategoryApi]);
        }

        public async Task<PagedList<RecipeCategoryModel>?> SearchAsync(QueryParameters? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters.PageSize)] = queryParameters == null ? int.MaxValue.ToString() : queryParameters.PageSize.ToString(),
                [nameof(QueryParameters.PageNumber)] = queryParameters == null ? "1" : queryParameters.PageNumber.ToString()
            };

            var response = await _httpClient.GetAsync(QueryHelpers.AddQueryString($"{_apiConfig?.Endpoints![ApiEndpointNames.RecipeCategoryApi]}/search", query));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PagedList<RecipeCategoryModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<string?> AddAsync(EditRecipeCategoryModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_apiConfig?.Endpoints![ApiEndpointNames.RecipeCategoryApi], modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }

        public async Task<string?> UpdateAsync(EditRecipeCategoryModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(_apiConfig?.Endpoints![ApiEndpointNames.RecipeCategoryApi], modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }

        public async Task<string?> UpdateAsync(IList<RecipeCategoryModel> models)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(models), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_apiConfig?.Endpoints![ApiEndpointNames.RecipeCategoryApi]}/updateAll", modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }

        public async Task<string?> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiConfig?.Endpoints![ApiEndpointNames.RecipeCategoryApi]}/{id}");
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }
    }
}
