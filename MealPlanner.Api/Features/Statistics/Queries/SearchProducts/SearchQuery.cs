using Common.Shared;
using MediatR;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Features.Statistics.Queries.SearchProducts
{
    public class SearchQuery : IRequest<IList<StatisticModel>>
    {
        public IList<ProductCategoryModel>? Categories { get; set; }
    }
}
