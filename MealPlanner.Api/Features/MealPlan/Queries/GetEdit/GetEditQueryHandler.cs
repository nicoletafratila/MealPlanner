using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetEdit
{
    public class GetEditQueryHandler(IMealPlanRepository repository, IMapper mapper) : IRequestHandler<GetEditMealPlanQuery, MealPlanEditModel>
    {
        private readonly IMealPlanRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<MealPlanEditModel> Handle(GetEditMealPlanQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdAsync(request.Id);
            return _mapper.Map<MealPlanEditModel>(result);
        }
    }
}
