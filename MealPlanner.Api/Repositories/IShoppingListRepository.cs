using Common.Data.Entities;
using Common.Data.Repository;

namespace MealPlanner.Api.Repositories
{
    public interface IShoppingListRepository : IAsyncRepository<ShoppingList, int>
    {
        Task<ShoppingList?> GetByIdIncludeProductsAsync(int id);
    }
}
