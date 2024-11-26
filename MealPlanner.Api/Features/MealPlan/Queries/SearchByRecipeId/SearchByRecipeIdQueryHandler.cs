using AutoMapper;
using Common.Models;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.SearchByRecipeId
{
    public class SearchByRecipeIdQueryHandler(IMealPlanRepository repository, IMapper mapper) : IRequestHandler<SearchByRecipeIdQuery, IList<MealPlanModel>>
    {
        private readonly IMealPlanRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<IList<MealPlanModel>> Handle(SearchByRecipeIdQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.SearchByRecipeAsync(request.RecipeId);
            var results= _mapper.Map<IList<MealPlanModel>>(data).ToList();
            results.SetIndexes();
            return results;
        }
    }
}
