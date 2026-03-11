using Common.Models;

namespace Common.Logging
{
    public interface ILoggerService
    {
        public Task<IEnumerable<LogModel>> GetLogsAsync(CancellationToken cancellationToken = default);
        public Task<LogModel?> GetLogAsync(int id, CancellationToken cancellationToken = default);
        public Task DeleteLogsAsync(CancellationToken cancellationToken = default);
    }
}
