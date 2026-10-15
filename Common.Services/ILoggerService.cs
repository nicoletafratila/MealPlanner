using Common.Models;
using Common.Pagination;

namespace Common.Services
{
    public interface ILoggerService
    {
        Task<IEnumerable<LogModel>> GetLogsAsync(CancellationToken cancellationToken = default);
        Task<LogModel?> GetLogAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedList<LogModel>> SearchAsync(QueryParameters<LogModel> queryParameters, CancellationToken cancellationToken = default);
        Task DeleteLogsAsync(CancellationToken cancellationToken = default);
    }
}
