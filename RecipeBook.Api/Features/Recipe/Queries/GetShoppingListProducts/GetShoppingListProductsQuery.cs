using MealPlanner.Shared.Models;
using MediatR;

namespace RecipeBook.Api.Features.Recipe.Queries.GetShoppingListProducts
{
    public class GetShoppingListProductsQuery : IRequest<IList<ShoppingListProductEditModel>?>
    {
        public int RecipeId { get; set; }
        public int ShopId { get; set; }
        public string? AuthToken { get; set; }
    }
}
