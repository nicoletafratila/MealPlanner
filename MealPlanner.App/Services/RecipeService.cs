﻿using RecipeBook.Shared.Models;
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
            return await JsonSerializer.DeserializeAsync<IEnumerable<RecipeModel>>
                    (await _httpClient.GetStreamAsync($"api/recipe"), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        }

        public async Task<EditRecipeModel> Get(int id)
        {
            return await JsonSerializer.DeserializeAsync<EditRecipeModel>
                    (await _httpClient.GetStreamAsync($"api/recipe/{id}"), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
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