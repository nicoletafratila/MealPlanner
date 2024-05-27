using AutoMapper;
using Common.Pagination;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.Search
{
    public class SearchQueryHandler(IMealPlanRepository repository, IMapper mapper) : IRequestHandler<SearchQuery, PagedList<MealPlanModel>>
    {
        private readonly IMealPlanRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<PagedList<MealPlanModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetAllAsync();
            var results = _mapper.Map<IList<MealPlanModel>>(data).OrderBy(r => r.Name).ToList();
            return results.ToPagedList(request.QueryParameters!.PageNumber, request.QueryParameters.PageSize);
        }
    }
}
