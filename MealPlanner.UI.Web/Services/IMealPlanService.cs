using Common.Pagination;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IMealPlanService
    {
        Task<EditMealPlanModel?> GetEditAsync(int id);
        Task<PagedList<MealPlanModel>?> SearchAsync(QueryParameters? queryParameters = null);
        Task<EditMealPlanModel?> AddAsync(EditMealPlanModel model);
        Task UpdateAsync(EditMealPlanModel model);
        Task DeleteAsync(int id);
    }
}
