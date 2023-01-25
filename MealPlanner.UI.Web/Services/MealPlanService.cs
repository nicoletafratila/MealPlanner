using Common.Constants;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class MealPlanService : IMealPlanService
    {
        private readonly HttpClient _httpClient;

        public MealPlanService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IList<MealPlanModel>> GetAll()
        {
            return await _httpClient.GetFromJsonAsync<IList<MealPlanModel>>(ApiNames.MealPlanApi);
        }

        public async Task<EditMealPlanModel> Get(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditMealPlanModel>($"{ApiNames.MealPlanApi}/{id}");
        }
    }
}
