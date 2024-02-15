using Common.Shared;
using MediatR;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Features.Statistics.Queries.GetFavoriteProducts
{
    public class GetFavoriteProductsQuery : IRequest<IList<StatisticModel>>
    {
        public IList<ProductCategoryModel>? Categories { get; set; }
    }
}
