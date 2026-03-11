using AutoMapper;
using Common.Pagination;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Queries.Search
{
    public class SearchQueryHandler(IShopRepository repository, IMapper mapper) : IRequestHandler<SearchQuery, PagedList<ShopModel>>
    {
        private readonly IShopRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<PagedList<ShopModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            if (request?.QueryParameters is null)
            {
                return new([], new Metadata());
            }

            var qp = request.QueryParameters;

            var entities = await _repository.GetAllAsync(cancellationToken);
            var models = _mapper.Map<IList<ShopModel>>(entities) ?? [];

            models = ApplyFilters(models, qp);
            models = ApplySorting(models, qp);

            return models.ToPagedList(qp.PageNumber, qp.PageSize);
        }

        private static IList<ShopModel> ApplyFilters(
            IList<ShopModel> source,
            QueryParameters<ShopModel> parameters)
        {
            if (parameters.Filters is null || !parameters.Filters.Any())
                return source;

            var result = source;

            foreach (var filter in parameters.Filters)
            {
                var predicate = filter.ConvertFilterItemToFunc<ShopModel>();
                result = result.Where(predicate).ToList();
            }

            return result;
        }

        private static IList<ShopModel> ApplySorting(
            IList<ShopModel> source,
            QueryParameters<ShopModel> parameters)
        {
            if (parameters.Sorting is null || !parameters.Sorting.Any())
                return source;

            var sortingItems = parameters.Sorting
                .Select(QueryParameters<ShopModel>.FromModel)
                .ToList();

            return source.AsQueryable()
                         .ApplySorting(sortingItems)!
                         .ToList();
        }
    }
}