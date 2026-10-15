using AutoMapper;
using Common.Data.Repository;
using Common.Models;
using Common.Pagination;

namespace Common.Services
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

        public async Task<PagedList<LogModel>> SearchAsync(QueryParameters<LogModel> queryParameters, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(queryParameters);

            var logs = await _repository.GetAllAsync(cancellationToken).ConfigureAwait(false);

            IList<LogModel> models = (logs ?? [])
                .Select(x => _mapper.Map<LogModel>(x))
                .ToList();

            models = ApplyFilters(models, queryParameters);
            models = ApplySorting(models, queryParameters);

            return models.ToPagedList(queryParameters.PageNumber, queryParameters.PageSize);
        }

        private static IList<LogModel> ApplyFilters(IList<LogModel> source, QueryParameters<LogModel> parameters)
        {
            if (parameters.Filters is null || !parameters.Filters.Any())
                return source;

            var result = source;

            foreach (var filter in parameters.Filters)
            {
                var predicate = filter.ConvertFilterItemToFunc<LogModel>();
                result = result.Where(predicate).ToList();
            }

            return result;
        }

        private static IList<LogModel> ApplySorting(IList<LogModel> source, QueryParameters<LogModel> parameters)
        {
            if (parameters.Sorting is null || !parameters.Sorting.Any())
                return source.OrderByDescending(x => x.Timestamp).ToList();

            return source.AsQueryable().ApplySorting(parameters.Sorting)!.ToList();
        }

        public async Task<LogModel?> GetLogAsync(Guid id, CancellationToken cancellationToken = default)
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
