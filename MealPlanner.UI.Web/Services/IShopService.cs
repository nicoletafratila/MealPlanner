using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IShopService
    {
        Task<IList<ShopModel>?> GetAllAsync();
    }
}
