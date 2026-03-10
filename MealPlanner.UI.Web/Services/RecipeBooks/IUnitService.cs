using Common.Models;
using Common.Pagination;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services.RecipeBooks
{
    public interface IUnitService
    {
        Task<UnitEditModel?> GetEditAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedList<UnitModel>?> SearchAsync(QueryParameters<UnitModel>? queryParameters = null, CancellationToken cancellationToken = default);
        Task<CommandResponse?> AddAsync(UnitEditModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> UpdateAsync(UnitEditModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
