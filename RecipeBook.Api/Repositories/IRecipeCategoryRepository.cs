using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    public interface IRecipeCategoryRepository : IAsyncRepository<RecipeCategory, int>
    {
        Task UpdateAllAsync(IList<RecipeCategory> entities);
    }
}
