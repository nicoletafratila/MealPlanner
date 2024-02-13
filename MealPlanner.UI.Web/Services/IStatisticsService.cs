using Common.Shared;

namespace MealPlanner.UI.Web.Services
{
    public interface IStatisticsService
    {
        Task<StatisticModel?> GetFavoriteRecipesAsync(int categoryId);
        Task<StatisticModel?> GetFavoriteProductsAsync(int categoryId);
    }
}
