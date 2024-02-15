using Common.Data.Entities;

namespace RecipeBook.Api.Repositories
{
    public interface IRecipeIngredientRepository
    {
        Task<IReadOnlyList<RecipeIngredient>?> SearchAsync(int productId);
    }
}
