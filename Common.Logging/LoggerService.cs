using AutoMapper;
using Common.Data.Repository;
using Common.Models;

namespace Common.Logging
{
    public class LoggerService(IMapper mapper, ILoggerRepository repository) : ILoggerService
    {
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILoggerRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        public async Task<IEnumerable<LogModel>> GetLogsAsync(CancellationToken cancellationToken = default)
        {
            var logs = await _repository.GetAllAsync(cancellationToken).ConfigureAwait(false);

            if (logs is null)
            {
                return [];
            }

            return logs
                .Select(x => _mapper.Map<LogModel>(x))
                .OrderByDescending(x => x.Timestamp)
                .ToArray();
        }

        public async Task<LogModel?> GetLogAsync(int id, CancellationToken cancellationToken = default)
        {
            var log = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (log is null)
            {
                return null;
            }

            return _mapper.Map<LogModel>(log);
        }

        public async Task DeleteLogsAsync(CancellationToken cancellationToken = default)
        {
            var logs = await _repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            if (logs is null)
            {
                return;
            }

            foreach (var item in logs)
            {
                if (item is null)
                {
                    continue;
                }

                await _repository.DeleteAsync(item, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}