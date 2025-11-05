using System.Text;
using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using Common.Models;
using Common.Pagination;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services.RecipeBooks
{
    public class RecipeCategoryService(HttpClient httpClient, TokenProvider tokenProvider) : IRecipeCategoryService
    {
        private readonly IApiConfig _recipeBookApiConfig = ServiceLocator.Current.GetInstance<RecipeBookApiConfig>();

        public async Task<RecipeCategoryEditModel?> GetEditAsync(int id)
        {
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            return await httpClient.GetFromJsonAsync<RecipeCategoryEditModel?>($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.RecipeCategory]}/edit/{id}");
        }

        public async Task<PagedList<RecipeCategoryModel>?> SearchAsync(QueryParameters<RecipeCategoryModel>? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters<RecipeCategoryModel>.Filters)] = queryParameters == null || queryParameters?.Filters == null ? null : JsonConvert.SerializeObject(queryParameters?.Filters),
                [nameof(QueryParameters<RecipeCategoryModel>.Sorting)] = queryParameters == null || queryParameters?.Sorting == null ? null : JsonConvert.SerializeObject(queryParameters?.Sorting),
                [nameof(QueryParameters<RecipeCategoryModel>.PageSize)] = queryParameters == null ? int.MaxValue.ToString() : queryParameters.PageSize.ToString(),
                [nameof(QueryParameters<RecipeCategoryModel>.PageNumber)] = queryParameters == null ? "1" : queryParameters.PageNumber.ToString()
            };

            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var response = await httpClient.GetAsync(QueryHelpers.AddQueryString($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.RecipeCategory]}/search", query));
            return JsonConvert.DeserializeObject<PagedList<RecipeCategoryModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> AddAsync(RecipeCategoryEditModel model)
        {
            var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var response = await httpClient.PostAsync(_recipeBookApiConfig?.Controllers![RecipeBookControllers.RecipeCategory], modelJson);
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> UpdateAsync(RecipeCategoryEditModel model)
        {
            var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var response = await httpClient.PutAsync(_recipeBookApiConfig?.Controllers![RecipeBookControllers.RecipeCategory], modelJson);
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> UpdateAsync(IList<RecipeCategoryModel> models)
        {
            var modelJson = new StringContent(JsonConvert.SerializeObject(models), Encoding.UTF8, "application/json");
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var response = await httpClient.PutAsync($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.RecipeCategory]}/updateAll", modelJson);
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> DeleteAsync(int id)
        {
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var response = await httpClient.DeleteAsync($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.RecipeCategory]}/{id}");
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }
    }
}
