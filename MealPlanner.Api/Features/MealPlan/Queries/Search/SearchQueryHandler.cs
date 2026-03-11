using AutoMapper;
using Common.Pagination;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.Search
{
    public class SearchQueryHandler(IMealPlanRepository repository, IMapper mapper) : IRequestHandler<SearchQuery, PagedList<MealPlanModel>>
    {
        private readonly IMealPlanRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<PagedList<MealPlanModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            if (request?.QueryParameters is null)
            {
                return new([], new Metadata());
            }

            var qp = request.QueryParameters;

            var entities = await _repository.GetAllAsync(cancellationToken);
            var models = _mapper.Map<IList<MealPlanModel>>(entities) ?? [];

            models = ApplyFilters(models, qp);
            models = ApplySorting(models, qp);

            return models.ToPagedList(qp.PageNumber, qp.PageSize);
        }

        private static IList<MealPlanModel> ApplyFilters(
            IList<MealPlanModel> source,
            QueryParameters<MealPlanModel> parameters)
        {
            if (parameters.Filters is null || !parameters.Filters.Any())
                return source;

            var result = source;

            foreach (var filter in parameters.Filters)
            {
                var predicate = filter.ConvertFilterItemToFunc<MealPlanModel>();
                result = result.Where(predicate).ToList();
            }

            return result;
        }

        private static IList<MealPlanModel> ApplySorting(
            IList<MealPlanModel> source,
            QueryParameters<MealPlanModel> parameters)
        {
            if (parameters.Sorting is null || !parameters.Sorting.Any())
                return source;

            var sortingItems = parameters.Sorting
                .Select(QueryParameters<MealPlanModel>.FromModel)
                .ToList();

            return source.AsQueryable()
                         .ApplySorting(sortingItems)!
                         .ToList();
        }
    }
}