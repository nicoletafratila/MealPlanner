using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    public interface IIngredientRepository : IAsyncRepository<Ingredient, int>
    { }
}
