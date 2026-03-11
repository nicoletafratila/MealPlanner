using AutoMapper;
using Common.Data.Repository;
using Common.Models;

namespace Common.Logging
{
    public class LoggerService(IMapper mapper, ILoggerRepository repository) : ILoggerService
    {
        public async Task<IEnumerable<LogModel>> GetLogsAsync(CancellationToken cancellationToken = default)
        {
            var logs = await repository.GetAllAsync(cancellationToken);
            return logs!.Select(x => mapper.Map<LogModel>(x)).OrderByDescending(x => x.Timestamp);
        }

        public async Task<LogModel?> GetLogAsync(int id, CancellationToken cancellationToken = default)
        {
            var log = await repository.GetByIdAsync(id, cancellationToken);
            return mapper.Map<LogModel>(log);
        }

        public async Task DeleteLogsAsync(CancellationToken cancellationToken = default)
        {
            var logs = await repository.GetAllAsync(cancellationToken);
            foreach (var item in logs!)
            {
                await repository!.DeleteAsync(item!, cancellationToken);
            }
        }
    }
}
