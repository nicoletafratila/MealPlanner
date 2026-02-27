using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository
{
    /// <summary>
    /// Generic async repository for basic CRUD operations.
    /// </summary>
    public class BaseAsyncRepository<T, TId>(DbContext dbContext) : IAsyncRepository<T, TId>
        where T : Entity<TId>
    {
        protected readonly DbContext DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public virtual async Task<T?> GetByIdAsync(TId id)
        {
            return await DbContext.Set<T>().FindAsync(id);
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await DbContext.Set<T>().ToListAsync();
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            await DbContext.Set<T>().AddAsync(entity);
            await DbContext.SaveChangesAsync();
            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            DbContext.Entry(entity).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            DbContext.Set<T>().Remove(entity);
            await DbContext.SaveChangesAsync();
        }
    }
}