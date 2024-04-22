using Common.Data.Entities;

namespace Common.Data.Repository
{
    public interface ILoggerRepository : IAsyncRepository<Log, int>
    {
    }
}
