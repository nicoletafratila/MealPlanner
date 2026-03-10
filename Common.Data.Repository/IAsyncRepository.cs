using Common.Data.Entities;

namespace Common.Data.Repository
{
    public interface IAsyncRepository<T, in TId> where T : Entity<TId>
    {
        /// <summary>
        /// Gets an entity by its id or null if not found.
        /// </summary>
        Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all entities. Returns an empty list when none exist.
        /// </summary>
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new entity and returns the persisted instance.
        /// </summary>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an existing entity.
        /// </summary>
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    }
}
