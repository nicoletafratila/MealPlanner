using Common.Data.Entities;
using Common.Data.Repository;

namespace MealPlanner.Api.Repositories
{
    public interface IShopRepository : IAsyncRepository<Shop, int>
    {
        Task<Shop?> GetByIdIncludeDisplaySequenceAsync(int id);
    }
}
