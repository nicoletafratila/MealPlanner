using AutoMapper;
using Common.Models;
using Common.Services;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.SearchByRecipeId
{
    /// <summary>
    /// Handles retrieving all meal plans that contain a given recipe.
    /// </summary>
    public class SearchByRecipeIdQueryHandler(IMealPlanRepository repository, IMapper mapper, ICurrentUserService currentUserService) : IRequestHandler<SearchByRecipeIdQuery, IList<MealPlanModel>>
    {
        private readonly IMealPlanRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

        public async Task<IList<MealPlanModel>> Handle(SearchByRecipeIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return [];

            var data = await _repository.SearchByRecipeAsync(request.RecipeId, userId, cancellationToken);
            var results = _mapper.Map<IList<MealPlanModel>>(data) ?? [];
            results.SetIndexes();
            return results;
        }
    }
}