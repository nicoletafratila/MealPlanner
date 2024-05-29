using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IUnitService
    {
        Task<UnitEditModel?> GetEditAsync(int id);
        Task<IList<UnitModel>?> GetAllAsync();
        Task<string?> AddAsync(UnitEditModel model);
        Task<string?> UpdateAsync(UnitEditModel model);
        Task<string?> DeleteAsync(int id);
    }
}
