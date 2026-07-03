using Common.Data.Repository;
using MealPlanner.Data.Entities;

namespace MealPlanner.Api.Repositories
{
    public interface IShopRepository : IAsyncRepository<Shop, Guid>
    {
        Task<IReadOnlyList<Shop>> GetAllByUserAsync(string userId, CancellationToken cancellationToken);
        Task<Shop?> GetByIdIncludeDisplaySequenceAsync(Guid? id, CancellationToken cancellationToken);
    }
}
