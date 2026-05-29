using Common.Data.Repository;
using MealPlanner.Data.Entities;

namespace MealPlanner.Api.Repositories
{
    public interface IShopRepository : IAsyncRepository<Shop, int>
    {
        Task<IReadOnlyList<Shop>> GetAllByUserAsync(string userId, CancellationToken cancellationToken);
        Task<Shop?> GetByIdIncludeDisplaySequenceAsync(int? id, CancellationToken cancellationToken);
    }
}
