using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    public interface IUnitRepository : IAsyncRepository<Unit, int>
    {
    }
}
