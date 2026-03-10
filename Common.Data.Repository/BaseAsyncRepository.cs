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

        public virtual async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        {
            // Use the FindAsync overload that accepts a CancellationToken
            return await DbContext.Set<T>().FindAsync([id!], cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<T>().ToListAsync(cancellationToken);
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);

            await DbContext.Set<T>().AddAsync(entity, cancellationToken);
            await DbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);

            DbContext.Entry(entity).State = EntityState.Modified;
            await DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);

            DbContext.Set<T>().Remove(entity);
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }
}