using Common.Shared;

namespace MealPlanner.UI.Web.Services
{
    public interface IStatisticsService
    {
        Task<IList<StatisticModel>?> GetFavoriteRecipesAsync();
        Task<IList<StatisticModel>?> GetFavoriteProductsAsync();
    }
}
