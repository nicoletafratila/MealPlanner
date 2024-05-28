using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Queries.GetAll
{
    public class GetAllQuery : IRequest<IList<ShopModel>>
    {
    }
}
