using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;
namespace MealPlanner.Services.Http
{
    public interface IShoppingListService
    {
        Task<ShoppingListEditModel?> GetEditAsync(Guid id, CancellationToken cancellationToken = default);

        Task<PagedList<ShoppingListModel>?> SearchAsync(
            QueryParameters<ShoppingListModel>? queryParameters = null,
            CancellationToken cancellationToken = default);

        Task<ShoppingListEditModel?> MakeShoppingListAsync(
            ShoppingListCreateModel model,
            CancellationToken cancellationToken = default);

        Task<CommandResponse?> AddAsync(
            ShoppingListEditModel model,
            CancellationToken cancellationToken = default);

        Task<CommandResponse?> UpdateAsync(
            ShoppingListEditModel model,
            CancellationToken cancellationToken = default);

        Task<CommandResponse?> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
