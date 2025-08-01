using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetEdit
{
    public class GetEditQueryHandler(IMealPlanRepository repository, IMapper mapper) : IRequestHandler<GetEditMealPlanQuery, MealPlanEditModel>
    {
        public async Task<MealPlanEditModel> Handle(GetEditMealPlanQuery request, CancellationToken cancellationToken)
        {
            var result = await repository.GetByIdAsync(request.Id);
            return mapper.Map<MealPlanEditModel>(result);
        }
    }
}
