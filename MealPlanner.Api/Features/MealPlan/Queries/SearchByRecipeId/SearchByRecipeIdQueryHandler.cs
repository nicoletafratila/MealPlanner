using AutoMapper;
using Common.Models;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.SearchByRecipeId
{
    /// <summary>
    /// Handles retrieving all meal plans that contain a given recipe.
    /// </summary>
    public class SearchByRecipeIdQueryHandler(IMealPlanRepository repository, IMapper mapper) : IRequestHandler<SearchByRecipeIdQuery, IList<MealPlanModel>>
    {
        private readonly IMealPlanRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<IList<MealPlanModel>> Handle(SearchByRecipeIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var data = await _repository.SearchByRecipeAsync(request.RecipeId, cancellationToken);
            var results = _mapper.Map<IList<MealPlanModel>>(data) ?? [];
            results.SetIndexes();
            return results;
        }
    }
}