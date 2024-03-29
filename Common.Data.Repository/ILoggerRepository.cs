using Common.Data.Entities;

namespace Common.Data.Repository
{
    public interface ILoggerRepository : IAsyncRepository<Log, int>
    {
        //Task<Log?> GetByIdAsync(int id);
        //Task<IReadOnlyList<Log>?> GetAllAsync();
        //Task DeleteAsync(Log entity);
    }
}
