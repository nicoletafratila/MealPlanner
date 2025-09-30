using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IShopService
    {
        Task<ShopEditModel?> GetEditAsync(int id);
        Task<PagedList<ShopModel>?> SearchAsync(QueryParameters<ShopModel>? queryParameters = null);
        Task<CommandResponse?> AddAsync(ShopEditModel model);
        Task<CommandResponse?> UpdateAsync(ShopEditModel model);
        Task<CommandResponse?> DeleteAsync(int id);
    }
}
