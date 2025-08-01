using AutoMapper;
using Common.Data.Repository;
using Common.Models;

namespace Common.Logging
{
    public class LoggerService(IMapper mapper, ILoggerRepository repository) : ILoggerService
    {
        public async Task<IEnumerable<LogModel>> GetLogsAsync()
        {
            var logs = await repository.GetAllAsync();
            return logs!.Select(x => mapper.Map<LogModel>(x)).OrderByDescending(x => x.Timestamp);
        }

        public async Task<LogModel?> GetLogAsync(int id)
        {
            var log = await repository.GetByIdAsync(id);
            return mapper.Map<LogModel>(log);
        }

        public async Task DeleteLogsAsync()
        {
            var logs = await repository.GetAllAsync();
            foreach (var item in logs!)
            {
                await repository!.DeleteAsync(item!);
            }
        }
    }
}
