using Common.Models;

namespace Common.Logging
{
    public interface ILoggerService
    {
        Task<IEnumerable<LogModel>> GetLogsAsync(CancellationToken cancellationToken = default);
        Task<LogModel?> GetLogAsync(int id, CancellationToken cancellationToken = default);
        Task DeleteLogsAsync(CancellationToken cancellationToken = default);
    }
}