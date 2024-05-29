using Common.Data.Entities;

namespace RecipeBook.Api.Repositories
{
    public interface IRecipeIngredientRepository 
    {
        Task<IReadOnlyList<RecipeIngredient>?> GetAllAsync();
        Task<IReadOnlyList<RecipeIngredient>?> SearchAsync(int productId);
    }
}
