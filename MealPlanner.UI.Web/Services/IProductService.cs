using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IProductService
    {
        Task<IList<ProductModel>?> GetAllAsync();
        Task<EditProductModel?> GetByIdAsync(int id);
        Task<IList<ProductModel>?> SearchAsync(int categoryId);
        Task<EditProductModel?> AddAsync(EditProductModel model);
        Task UpdateAsync(EditProductModel model);
        Task DeleteAsync(int id);
    }
}
