using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Queries.GetShops
{
    public class GetShopsQuery : IRequest<IList<ShopModel>>
    {
    }
}
