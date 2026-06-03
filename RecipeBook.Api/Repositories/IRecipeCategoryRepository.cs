using Common.Data.Repository;
using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Repository contract for querying and updating <see cref="RecipeCategory"/> entities.
    /// </summary>
    public interface IRecipeCategoryRepository : IAsyncRepository<RecipeCategory, Guid>
    {
        Task<IReadOnlyList<RecipeCategory>> GetAllByUserAsync(string userId, CancellationToken cancellationToken);

        Task<IReadOnlyList<RecipeCategory>> GetByIdsAsync(IList<Guid> ids, CancellationToken cancellationToken);

        /// <summary>
        /// Updates all provided recipe categories in a single save operation.
        /// </summary>
        Task UpdateAllAsync(IList<RecipeCategory> entities, CancellationToken cancellationToken);
    }
}