using AutoMapper;
using Common.Models;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetAll
{
    public class GetAllQueryHandler(IMealPlanRepository repository, IMapper mapper) : IRequestHandler<GetAllQuery, IList<MealPlanModel>>
    {
        private readonly IMealPlanRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<IList<MealPlanModel>> Handle(GetAllQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetAllAsync();
            var results = _mapper.Map<IList<MealPlanModel>>(data).OrderBy(r => r.Name).ToList();
            results.SetIndexes();
            return results;
        }
    }
}
