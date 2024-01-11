using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IShopService
    {
        Task<EditShopModel?> GetEditAsync(int id);
        Task<IList<ShopModel>?> GetAllAsync();
        Task<string?> AddAsync(EditShopModel model);
        Task<string?> UpdateAsync(EditShopModel model);
        Task<string?> DeleteAsync(int id);
    }
}
