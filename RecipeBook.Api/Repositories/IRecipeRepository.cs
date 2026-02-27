using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Repository contract for querying and manipulating <see cref="Recipe"/> entities.
    /// </summary>
    public interface IRecipeRepository : IAsyncRepository<Recipe, int>
    {
        /// <summary>
        /// Gets a recipe by id including its category and ingredients graph, or null if not found.
        /// </summary>
        Task<Recipe?> GetByIdIncludeIngredientsAsync(int? id);

        /// <summary>
        /// Gets all recipes in a given category.
        /// </summary>
        Task<IReadOnlyList<Recipe>> SearchAsync(int categoryId);

        /// <summary>
        /// Finds a recipe by name (case-insensitive), or null if not found.
        /// </summary>
        Task<Recipe?> SearchAsync(string name);
    }
}