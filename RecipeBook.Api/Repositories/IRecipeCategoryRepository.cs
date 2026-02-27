using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Repository contract for querying and updating <see cref="RecipeCategory"/> entities.
    /// </summary>
    public interface IRecipeCategoryRepository : IAsyncRepository<RecipeCategory, int>
    {
        /// <summary>
        /// Updates all provided recipe categories in a single save operation.
        /// </summary>
        Task UpdateAllAsync(IList<RecipeCategory> entities);
    }
}