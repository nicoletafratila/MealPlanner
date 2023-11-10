using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.DeleteShoppingList
{
    public class DeleteShoppingListCommand : IRequest<DeleteShoppingListCommandResponse>
    {
        public int Id { get; set; }
    }
}
