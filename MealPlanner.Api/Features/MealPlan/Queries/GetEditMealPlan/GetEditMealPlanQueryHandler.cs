using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetMealPlan
{
    public class GetEditMealPlanQueryHandler(IMealPlanRepository repository, IMapper mapper) : IRequestHandler<GetEditMealPlanQuery, EditMealPlanModel>
    {
        private readonly IMealPlanRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<EditMealPlanModel> Handle(GetEditMealPlanQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdAsync(request.Id);
            return _mapper.Map<EditMealPlanModel>(result);
        }
    }
}
