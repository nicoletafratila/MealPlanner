using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Queries.GetEdit
{
    /// <summary>
    /// Query to retrieve a shopping list for editing.
    /// </summary>
    public class GetEditQuery : IRequest<ShoppingListEditModel>
    {
        /// <summary>
        /// Id of the shopping list to edit.
        /// </summary>
        public int Id { get; set; }

        public GetEditQuery()
        {
        }

        public GetEditQuery(int id)
        {
            Id = id;
        }
    }
}