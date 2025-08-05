using System.Text;
using BlazorBootstrap;
using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using Common.Models;
using Common.Pagination;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class ProductService(HttpClient httpClient) : IProductService
    {
        private readonly IApiConfig _recipeBookApiConfig = ServiceLocator.Current.GetInstance<RecipeBookApiConfig>();

        public async Task<ProductEditModel?> GetEditAsync(int id)
        {
            return await httpClient.GetFromJsonAsync<ProductEditModel?>($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.Product]}/edit/{id}");
        }

        public async Task<PagedList<ProductModel>?> SearchAsync(QueryParameters? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters.Filters)] = queryParameters == null || queryParameters?.Filters == null ? null :JsonConvert.SerializeObject(queryParameters?.Filters),
                [nameof(QueryParameters.SortString)] = queryParameters == null ? "Name" : queryParameters?.SortString?.ToString(),
                [nameof(QueryParameters.SortDirection)] = queryParameters == null ? SortDirection.Ascending.ToString() : queryParameters.SortDirection.ToString(),
                [nameof(QueryParameters.PageSize)] = queryParameters == null ? int.MaxValue.ToString() : queryParameters.PageSize.ToString(),
                [nameof(QueryParameters.PageNumber)] = queryParameters == null ? "1" : queryParameters.PageNumber.ToString()
            };

            var response = await httpClient.GetAsync(QueryHelpers.AddQueryString($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.Product]}/search", query));
            return JsonConvert.DeserializeObject<PagedList<ProductModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> AddAsync(ProductEditModel model)
        {
            var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(_recipeBookApiConfig?.Controllers![RecipeBookControllers.Product], modelJson);
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> UpdateAsync(ProductEditModel model)
        {
            var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync(_recipeBookApiConfig?.Controllers![RecipeBookControllers.Product], modelJson);
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> DeleteAsync(int id)
        {
            var response = await httpClient.DeleteAsync($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.Product]}/{id}");
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }
    }
}
