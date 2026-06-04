using Common.Models;
using Common.Pagination;
using RecipeBook.Shared.Models;

namespace RecipeBook.Services.Http
{
    public interface IUnitService
    {
        Task<UnitEditModel?> GetEditAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedList<UnitModel>?> SearchAsync(QueryParameters<UnitModel>? queryParameters = null, CancellationToken cancellationToken = default);
        Task<CommandResponse?> AddAsync(UnitEditModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> UpdateAsync(UnitEditModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
