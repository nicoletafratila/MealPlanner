using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.SearchMealPlansByRecipeId
{
    public class SearchMealPlansByRecipeIdQueryHandler(IMealPlanRepository repository, IMapper mapper) : IRequestHandler<SearchMealPlansByRecipeIdQuery, IList<MealPlanModel>>
    {
        private readonly IMealPlanRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<IList<MealPlanModel>> Handle(SearchMealPlansByRecipeIdQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.SearchByRecipeAsync(request.RecipeId);
            return _mapper.Map<IList<MealPlanModel>>(data).ToList();
        }
    }
}
