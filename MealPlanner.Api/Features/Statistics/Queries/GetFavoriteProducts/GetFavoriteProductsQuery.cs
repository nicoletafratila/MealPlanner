using Common.Shared;
using MediatR;

namespace MealPlanner.Api.Features.Statistics.Queries.GetFavoriteProducts
{
    public class GetFavoriteProductsQuery : IRequest<IList<StatisticModel>>
    {
    }
}
