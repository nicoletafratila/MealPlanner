using Common.Data.Repository;
using Common.Pagination;
using MealPlanner.Data.Entities;

namespace MealPlanner.Api.Repositories
{
    public interface IShoppingListRepository : IAsyncRepository<ShoppingList, Guid>
    {
        Task<IReadOnlyList<ShoppingList>> GetAllByUserAsync(string userId, CancellationToken cancellationToken);
        Task<ShoppingList?> GetByIdIncludeProductsAsync(Guid id, CancellationToken cancellationToken);
        Task<ShoppingList?> SearchAsync(string name, string userId, CancellationToken cancellationToken);
        Task<bool> UpdateProductCollectedAsync(Guid shoppingListId, Guid productId, bool collected, CancellationToken cancellationToken);

        /// <summary>
        /// Filters, sorts, and pages shopping lists for a user at the database level, returning only the requested page.
        /// </summary>
        Task<PagedQueryResult<ShoppingList>> SearchByUserAsync(
            string userId,
            IEnumerable<FilterItem>? filters,
            IEnumerable<SortingModel>? sorting,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken);
    }
}
