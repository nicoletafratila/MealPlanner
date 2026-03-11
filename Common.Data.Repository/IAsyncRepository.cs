using Common.Data.Entities;

namespace Common.Data.Repository
{
    public interface IAsyncRepository<T, in TId> where T : Entity<TId>
    {
        /// <summary>
        /// Gets an entity by its id or null if not found.
        /// </summary>
        Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken);

        /// <summary>
        /// Gets all entities. Returns an empty list when none exist.
        /// </summary>
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Adds a new entity and returns the persisted instance.
        /// </summary>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        Task UpdateAsync(T entity, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes an existing entity.
        /// </summary>
        Task DeleteAsync(T entity, CancellationToken cancellationToken);
    }
}
