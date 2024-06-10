using AutoMapper;
using Common.Data.Repository;
using Common.Models;

namespace Common.Logging
{
    public class LoggerService(IMapper mapper, ILoggerRepository repository) : ILoggerService
    {
        private readonly IMapper _mapper = mapper;
        private readonly ILoggerRepository _repository = repository;

        public async Task<IEnumerable<LogModel>> GetLogsAsync()
        {
            var logs = await _repository.GetAllAsync();
            return logs!.Select(x => _mapper.Map<LogModel>(x)).OrderByDescending(x => x.Timestamp);
        }

        public async Task<LogModel?> GetLogAsync(int id)
        {
            var log = await _repository.GetByIdAsync(id);
            return _mapper.Map<LogModel>(log);
        }

        public async Task DeleteLogsAsync()
        {
            var logs = await _repository.GetAllAsync();
            foreach (var item in logs!)
            {
                await _repository!.DeleteAsync(item!);
            }
        }
    }
}
