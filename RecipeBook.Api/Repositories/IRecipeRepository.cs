using Common.Data.Repository;
using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Repository contract for querying and manipulating <see cref="Recipe"/> entities.
    /// </summary>
    public interface IRecipeRepository : IAsyncRepository<Recipe, Guid>
    {
        Task<IReadOnlyList<Recipe>> GetAllByUserAsync(string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a recipe by id including its category and ingredients graph, or null if not found.
        /// </summary>
        Task<Recipe?> GetByIdIncludeIngredientsAsync(Guid? id, CancellationToken cancellationToken);

        /// <summary>
        /// Gets all recipes in a given category.
        /// </summary>
        Task<IReadOnlyList<Recipe>> SearchAsync(Guid categoryId, CancellationToken cancellationToken);

        /// <summary>
        /// Finds a recipe by name (case-insensitive) scoped to a user, or null if not found.
        /// </summary>
        Task<Recipe?> SearchAsync(string name, string userId, CancellationToken cancellationToken);
    }
}