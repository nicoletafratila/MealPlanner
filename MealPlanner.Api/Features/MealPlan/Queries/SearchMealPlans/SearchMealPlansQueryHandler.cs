using AutoMapper;
using Common.Pagination;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.SearchMealPlans
{
    public class SearchMealPlansQueryHandler(IMealPlanRepository repository, IMapper mapper) : IRequestHandler<SearchMealPlansQuery, PagedList<MealPlanModel>>
    {
        private readonly IMealPlanRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<PagedList<MealPlanModel>> Handle(SearchMealPlansQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetAllAsync();
            var results = _mapper.Map<IList<MealPlanModel>>(data).OrderBy(r => r.Name).ToList();
            return results.ToPagedList(request.QueryParameters!.PageNumber, request.QueryParameters.PageSize);
        }
    }
}
