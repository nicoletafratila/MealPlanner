using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IShopService
    {
        Task<ShopModel?> GetByIdAsync(int id);
        Task<IList<ShopModel>?> GetAllAsync();
    }
}
