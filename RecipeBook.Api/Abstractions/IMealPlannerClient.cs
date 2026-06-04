using MealPlanner.Shared.Models;

namespace RecipeBook.Api.Abstractions
{
    public interface IMealPlannerClient
    {
        Task<ShopEditModel?> GetShopAsync(Guid shopId, string? authToken, CancellationToken cancellationToken);
        Task<IList<MealPlanModel>?> GetMealPlansByRecipeIdAsync(Guid recipeId, string? authToken, CancellationToken cancellationToken);
    }
}
