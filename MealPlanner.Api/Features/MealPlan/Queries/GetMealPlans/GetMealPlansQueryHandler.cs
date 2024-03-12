using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetMealPlans
{
    public class GetMealPlansQueryHandler(IMealPlanRepository repository, IMapper mapper) : IRequestHandler<GetMealPlansQuery, IList<MealPlanModel>>
    {
        private readonly IMealPlanRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<IList<MealPlanModel>> Handle(GetMealPlansQuery request, CancellationToken cancellationToken)
        {
            var results = await _repository.GetAllAsync();
            return _mapper.Map<IList<MealPlanModel>>(results).OrderBy(r => r.Name).ToList();
        }
    }
}
