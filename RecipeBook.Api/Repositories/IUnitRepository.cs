using Common.Data.Repository;
using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Repositories
{
    public interface IUnitRepository : IAsyncRepository<Unit, int>
    {
    }
}
