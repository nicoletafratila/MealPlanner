using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IUnitService
    {
        Task<EditUnitModel?> GetEditAsync(int id);
        Task<IList<UnitModel>?> GetAllAsync();
        Task<string?> AddAsync(EditUnitModel model);
        Task<string?> UpdateAsync(EditUnitModel model);
        Task<string?> DeleteAsync(int id);
    }
}
