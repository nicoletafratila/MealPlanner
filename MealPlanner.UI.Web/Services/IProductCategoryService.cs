using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IProductCategoryService
    {
        Task<IList<ProductCategoryModel>?> GetAllAsync();
    }
}
