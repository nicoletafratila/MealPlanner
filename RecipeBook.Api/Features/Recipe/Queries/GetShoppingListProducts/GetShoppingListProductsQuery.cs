using MealPlanner.Shared.Models;
using MediatR;

namespace RecipeBook.Api.Features.Recipe.Queries.GetShoppingListProducts
{
    /// <summary>
    /// Query to retrieve shopping-list products for a given recipe and shop.
    /// </summary>
    public class GetShoppingListProductsQuery : IRequest<IList<ShoppingListProductEditModel>?>
    {
        /// <summary>
        /// Id of the recipe to build a shopping list for.
        /// </summary>
        public int RecipeId { get; set; }

        /// <summary>
        /// Id of the shop/context for which to generate product quantities/order.
        /// </summary>
        public int ShopId { get; set; }

        /// <summary>
        /// Optional auth token (if needed for downstream services).
        /// </summary>
        public string? AuthToken { get; set; }

        public GetShoppingListProductsQuery()
        {
        }

        public GetShoppingListProductsQuery(int recipeId, int shopId, string? authToken = null)
        {
            RecipeId = recipeId;
            ShopId = shopId;
            AuthToken = authToken;
        }
    }
}