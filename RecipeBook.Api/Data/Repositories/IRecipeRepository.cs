using RecipeBook.Api.Data.Entities;

namespace RecipeBook.Api.Data.Repositories
{
    public interface IRecipeRepository : IAsyncRepository<Recipe, int>
    {
        Task<Recipe> GetByIdAsyncIncludeIngredients(int id);
    }
}
