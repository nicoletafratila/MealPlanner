using Common.Shared;

namespace Common.Logging
{
    public interface ILoggerService
    {
        public Task<IEnumerable<LogModel>> GetLogsAsync();
        public Task<LogModel?> GetLogAsync(int id);
        public Task DeleteLogsAsync();
    }
}
