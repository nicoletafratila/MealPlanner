using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Queries.GetEdit
{
    public class GetEditQuery : IRequest<EditShoppingListModel>
    {
        public int Id { get; set; }
    }
}
