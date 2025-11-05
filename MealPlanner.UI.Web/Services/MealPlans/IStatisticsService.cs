using Common.Models;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services.MealPlans
{
    public interface IStatisticsService
    {
        Task<IList<StatisticModel>?> GetFavoriteRecipesAsync(IList<RecipeCategoryModel> categories);
        Task<IList<StatisticModel>?> GetFavoriteProductsAsync(IList<ProductCategoryModel> categories);
    }
}
