using AutoMapper;
using Common.Pagination;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Queries.Search
{
    public class SearchQueryHandler(IUnitRepository repository, IMapper mapper) : IRequestHandler<SearchQuery, PagedList<UnitModel>>
    {
        private readonly IUnitRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<PagedList<UnitModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            if (request?.QueryParameters is null)
            {
                return EmptyResult();
            }

            var queryParameters = request.QueryParameters;

            var entities = await _repository.GetAllAsync();
            var models = _mapper.Map<IList<UnitModel>>(entities) ?? [];

            models = ApplyFilters(models, queryParameters);
            models = ApplySorting(models, queryParameters);

            return models.ToPagedList(queryParameters.PageNumber, queryParameters.PageSize);
        }

        private static IList<UnitModel> ApplyFilters(
            IList<UnitModel> source,
            QueryParameters<UnitModel> parameters)
        {
            if (parameters.Filters is null || !parameters.Filters.Any())
                return source;

            var result = source;

            foreach (var filter in parameters.Filters)
            {
                var predicate = filter.ConvertFilterItemToFunc<UnitModel>();
                result = result.Where(predicate).ToList();
            }

            return result;
        }

        private static IList<UnitModel> ApplySorting(
            IList<UnitModel> source,
            QueryParameters<UnitModel> parameters)
        {
            if (parameters.Sorting is null || !parameters.Sorting.Any())
                return source;

            var sortingItems = parameters.Sorting
                .Select(QueryParameters<UnitModel>.FromModel)
                .ToList();

            return source.AsQueryable().ApplySorting(sortingItems)!.ToList();
        }

        private static PagedList<UnitModel> EmptyResult()
            => new([], new Metadata());
    }
}