using MealPlanner.Shared.Models;

namespace RecipeBook.Api.Abstractions
{
    public interface IMealPlannerClient
    {
        Task<ShopEditModel?> GetShopAsync(int shopId, string? authToken, CancellationToken cancellationToken);
    }
}
