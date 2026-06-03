using System.Net.Http.Json;using Common.Models; using Common.Pagination; using MealPlanner.Shared.Models; using Microsoft.Extensions.Logging;

namespace MealPlanner.Services.Http
{
    public interface IShopService
    {
        Task<ShopEditModel?> GetEditAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedList<ShopModel>?> SearchAsync(
            QueryParameters<ShopModel>? queryParameters = null,
            CancellationToken cancellationToken = default);
        Task<CommandResponse?> AddAsync(ShopEditModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> UpdateAsync(ShopEditModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
