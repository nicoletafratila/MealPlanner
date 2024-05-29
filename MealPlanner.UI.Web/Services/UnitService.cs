using System.Text;
using System.Text.Json;
using Common.Api;
using Common.Constants;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class UnitService(HttpClient httpClient, IServiceProvider serviceProvider) : IUnitService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IApiConfig _apiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.RecipeBook);

        public async Task<UnitEditModel?> GetEditAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<UnitEditModel?>($"{_apiConfig?.Endpoints![ApiEndpointNames.UnitApi]}/edit/{id}");
        }

        public async Task<IList<UnitModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<UnitModel>?>(_apiConfig?.Endpoints![ApiEndpointNames.UnitApi]);
        }

        public async Task<string?> AddAsync(UnitEditModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_apiConfig?.Endpoints![ApiEndpointNames.UnitApi], modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }

        public async Task<string?> UpdateAsync(UnitEditModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(_apiConfig?.Endpoints![ApiEndpointNames.UnitApi], modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }

        public async Task<string?> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiConfig?.Endpoints![ApiEndpointNames.UnitApi]}/{id}");
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }
    }
}
