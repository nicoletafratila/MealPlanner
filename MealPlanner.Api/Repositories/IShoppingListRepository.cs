using MealPlanner.Data.Entities;
using Common.Data.Repository;

namespace MealPlanner.Api.Repositories
{
    public interface IShoppingListRepository : IAsyncRepository<ShoppingList, int>
    {
        Task<IReadOnlyList<ShoppingList>> GetAllByUserAsync(string userId, CancellationToken cancellationToken);
        Task<ShoppingList?> GetByIdIncludeProductsAsync(int id, CancellationToken cancellationToken);
        Task<ShoppingList?> SearchAsync(string name, string userId, CancellationToken cancellationToken);
    }
}
