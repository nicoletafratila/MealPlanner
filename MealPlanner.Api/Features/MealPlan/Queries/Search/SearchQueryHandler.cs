using AutoMapper;
using Common.Pagination;
using Common.Services;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.Search
{
    public class SearchQueryHandler(IMealPlanRepository repository, IMapper mapper, ICurrentUserService currentUserService) : IRequestHandler<SearchQuery, PagedList<MealPlanModel>>
    {
        private readonly IMealPlanRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

        public async Task<PagedList<MealPlanModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            if (request?.QueryParameters is null)
            {
                return new([], new Metadata());
            }

            var qp = request.QueryParameters;
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return new([], new Metadata());

            var (entities, totalCount, skip) = await _repository.SearchByUserAsync(
                userId, qp.Filters, qp.Sorting, qp.PageNumber, qp.PageSize, cancellationToken);

            var models = _mapper.Map<IList<MealPlanModel>>(entities) ?? [];
            var metadata = Metadata.Create(qp.PageNumber, qp.PageSize, totalCount);
            for (var i = 0; i < models.Count; i++)
                models[i].Index = skip + i + 1;

            return new PagedList<MealPlanModel>(models, metadata);
        }
    }
}
