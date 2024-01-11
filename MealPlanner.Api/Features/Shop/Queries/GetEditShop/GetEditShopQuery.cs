using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Queries.GetEditShop
{
    public class GetEditShopQuery : IRequest<EditShopModel>
    {
        public int Id { get; set; }
    }
}
