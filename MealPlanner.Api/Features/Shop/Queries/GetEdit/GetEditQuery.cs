using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Queries.GetEdit
{
    public class GetEditQuery : IRequest<EditShopModel>
    {
        public int Id { get; set; }
    }
}
