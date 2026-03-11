using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Queries.GetEdit
{
    /// <summary>
    /// Query to retrieve a shop for editing.
    /// </summary>
    public class GetEditQuery : IRequest<ShopEditModel>
    {
        /// <summary>
        /// Id of the shop to edit.
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