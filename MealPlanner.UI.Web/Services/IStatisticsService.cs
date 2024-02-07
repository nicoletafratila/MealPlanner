using Common.Shared;

namespace MealPlanner.UI.Web.Services
{
    public interface IStatisticsService
    {
        Task<StatisticModel?> GetFavoriteRecipesAsync(string? categoryId);
    }
}
