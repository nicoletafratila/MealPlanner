using Common.Data.Entities;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Repository contract for querying <see cref="RecipeIngredient"/> entities.
    /// </summary>
    public interface IRecipeIngredientRepository
    {
        /// <summary>
        /// Gets all recipe ingredients, typically including related unit or product data.
        /// </summary>
        Task<IReadOnlyList<RecipeIngredient>> GetAllAsync();

        /// <summary>
        /// Gets all recipe ingredients associated with the given product id.
        /// </summary>
        Task<IReadOnlyList<RecipeIngredient>> SearchAsync(int productId);
    }
}