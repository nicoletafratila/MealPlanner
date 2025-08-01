using Common.Models;
using Common.Pagination;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IUnitService
    {
        Task<UnitEditModel?> GetEditAsync(int id);
        Task<PagedList<UnitModel>?> SearchAsync(QueryParameters? queryParameters = null);
        Task<CommandResponse?> AddAsync(UnitEditModel model);
        Task<CommandResponse?> UpdateAsync(UnitEditModel model);
        Task<CommandResponse?> DeleteAsync(int id);
    }
}
