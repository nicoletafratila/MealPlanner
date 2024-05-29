using Common.Pagination;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IProductService
    {
        Task<ProductEditModel?> GetEditAsync(int id);
        Task<PagedList<ProductModel>?> SearchAsync(string? categoryId = null, QueryParameters? queryParameters = null);
        Task<string?> AddAsync(ProductEditModel model);
        Task<string?> UpdateAsync(ProductEditModel model);
        Task<string?> DeleteAsync(int id);
    }
}
