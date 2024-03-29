using Common.Data.Entities;

namespace Common.Data.Repository
{
    public interface IAsyncRepository<T, in TId> where T : Entity<TId>
    {
        Task<T?> GetByIdAsync(TId id);
        Task<IReadOnlyList<T>?> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}
