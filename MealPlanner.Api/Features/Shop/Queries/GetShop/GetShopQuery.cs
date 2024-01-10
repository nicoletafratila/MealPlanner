using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Queries.GetShop
{
    public class GetShopQuery : IRequest<ShopModel>
    {
        public int Id { get; set; }
    }
}
