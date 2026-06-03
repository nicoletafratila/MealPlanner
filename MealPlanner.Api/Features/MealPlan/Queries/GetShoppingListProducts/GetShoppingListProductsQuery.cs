using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetShoppingListProducts
{
    /// <summary>
    /// Query to retrieve shopping-list products generated from a meal plan for a given shop.
    /// </summary>
    public class GetShoppingListProductsQuery : IRequest<IList<ShoppingListProductEditModel>?>
    {
        public Guid MealPlanId { get; set; }
        public int ShopId { get; set; }

        public GetShoppingListProductsQuery()
        {
        }

        public GetShoppingListProductsQuery(Guid mealPlanId, int shopId)
        {
            MealPlanId = mealPlanId;
            ShopId = shopId;
        }
    }
}