using Common.Pagination;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IShopService
    {
        Task<ShopEditModel?> GetEditAsync(int id);
        Task<PagedList<ShopModel>?> SearchAsync(QueryParameters? queryParameters = null);
        Task<string?> AddAsync(ShopEditModel model);
        Task<string?> UpdateAsync(ShopEditModel model);
        Task<string?> DeleteAsync(int id);
    }
}
