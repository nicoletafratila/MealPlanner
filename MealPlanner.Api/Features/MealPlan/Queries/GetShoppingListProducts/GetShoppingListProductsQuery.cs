using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetShoppingListProducts
{
    public class GetShoppingListProductsQuery : IRequest<IList<ShoppingListProductModel>?>
    {
        public int MealPlanId { get; set; }
        public int ShopId { get; set; }
    }
}
