using AutoMapper;
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
            var results = await _repository.GetAllAsync();
            return _mapper.Map<IList<MealPlanModel>>(results).OrderBy(r => r.Name).ToList();
        }
    }
}
