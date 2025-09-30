using AutoMapper;
using Common.Pagination;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.Search
{
    public class SearchQueryHandler(IMealPlanRepository repository, IMapper mapper) : IRequestHandler<SearchQuery, PagedList<MealPlanModel>>
    {
        public async Task<PagedList<MealPlanModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var data = await repository.GetAllAsync();
            var results = mapper.Map<IList<MealPlanModel>>(data);

            if (results != null && request.QueryParameters != null)
            {
                if (request.QueryParameters.Filters != null)
                {
                    foreach (var filter in request.QueryParameters.Filters)
                    {
                        results = results.Where(filter.ConvertFilterItemToFunc<MealPlanModel>()).ToList();
                    }
                }

                if (request.QueryParameters!.Sorting != null && request.QueryParameters.Sorting.Any())
                {
                    var sortingItems = request.QueryParameters.Sorting.Select(QueryParameters<MealPlanModel>.FromModel).ToList();
                    results = results.AsQueryable().ApplySorting(sortingItems).ToList();
                }

                return results.ToPagedList(request.QueryParameters!.PageNumber, request.QueryParameters.PageSize);
            }

            return new PagedList<MealPlanModel>(new List<MealPlanModel>(), new Metadata());
        }
    }
}
