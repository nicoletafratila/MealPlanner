using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Queries.GetEditShoppingList
{
    public class GetEditShoppingListQuery : IRequest<EditShoppingListModel>
    {
        public int Id { get; set; }
    }
}
