﻿using System.Text;
using System.Text.Json;
using BlazorBootstrap;
using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using Common.Pagination;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace MealPlanner.UI.Web.Services
{
    public class ShoppingListService(HttpClient httpClient) : IShoppingListService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IApiConfig _mealPlannerApiConfig = ServiceLocator.Current.GetInstance<MealPlannerApiConfig>();

        public async Task<ShoppingListEditModel?> GetEditAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ShoppingListEditModel?>($"{_mealPlannerApiConfig?.Endpoints![ApiEndpointNames.ShoppingListApi]}/edit/{id}");
        }

        public async Task<PagedList<ShoppingListModel>?> SearchAsync(QueryParameters? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters.Filters)] = queryParameters == null || queryParameters?.Filters == null ? null : JsonSerializer.Serialize(queryParameters?.Filters),
                [nameof(QueryParameters.SortString)] = queryParameters == null ? "Name" : queryParameters?.SortString?.ToString(),
                [nameof(QueryParameters.SortDirection)] = queryParameters == null ? SortDirection.Ascending.ToString() : queryParameters.SortDirection.ToString(),
                [nameof(QueryParameters.PageSize)] = queryParameters == null ? int.MaxValue.ToString() : queryParameters.PageSize.ToString(),
                [nameof(QueryParameters.PageNumber)] = queryParameters == null ? "1" : queryParameters.PageNumber.ToString()
            };

            var response = await _httpClient.GetAsync(QueryHelpers.AddQueryString($"{_mealPlannerApiConfig?.Endpoints![ApiEndpointNames.ShoppingListApi]}/search", query));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PagedList<ShoppingListModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<ShoppingListEditModel?> MakeShoppingListAsync(ShoppingListCreateModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_mealPlannerApiConfig?.Endpoints![ApiEndpointNames.ShoppingListApi]}/makeShoppingList", modelJson);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ShoppingListEditModel?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<string?> AddAsync(ShoppingListEditModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_mealPlannerApiConfig?.Endpoints![ApiEndpointNames.ShoppingListApi], modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }

        public async Task<string?> UpdateAsync(ShoppingListEditModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(_mealPlannerApiConfig?.Endpoints![ApiEndpointNames.ShoppingListApi], modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }

        public async Task<string?> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_mealPlannerApiConfig?.Endpoints![ApiEndpointNames.ShoppingListApi]}/{id}");
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }
    }
}
