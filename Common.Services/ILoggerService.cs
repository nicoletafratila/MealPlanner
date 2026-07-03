using Common.Models;

namespace Common.Services
{
    public interface ILoggerService
    {
        Task<IEnumerable<LogModel>> GetLogsAsync(CancellationToken cancellationToken = default);
        Task<LogModel?> GetLogAsync(Guid id, CancellationToken cancellationToken = default);
        Task DeleteLogsAsync(CancellationToken cancellationToken = default);
    }
}
