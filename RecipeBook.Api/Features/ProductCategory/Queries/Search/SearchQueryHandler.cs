using AutoMapper;
using Common.Pagination;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.Search
{
    public class SearchQueryHandler(IProductCategoryRepository repository, IMapper mapper) : IRequestHandler<SearchQuery, PagedList<ProductCategoryModel>>
    {
        private readonly IProductCategoryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<PagedList<ProductCategoryModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            if (request?.QueryParameters is null)
            {
                return new([], new Metadata());
            }

            var qp = request.QueryParameters;

            var entities = await _repository.GetAllAsync(cancellationToken);
            var models = _mapper.Map<IList<ProductCategoryModel>>(entities) ?? [];

            models = ApplyFilters(models, qp);
            models = ApplySorting(models, qp);

            return models.ToPagedList(qp.PageNumber, qp.PageSize);
        }

        private static IList<ProductCategoryModel> ApplyFilters(
            IList<ProductCategoryModel> source,
            QueryParameters<ProductCategoryModel> parameters)
        {
            if (parameters.Filters is null || !parameters.Filters.Any())
                return source;

            var result = source;

            foreach (var filter in parameters.Filters)
            {
                var predicate = filter.ConvertFilterItemToFunc<ProductCategoryModel>();
                result = result.Where(predicate).ToList();
            }

            return result;
        }

        private static IList<ProductCategoryModel> ApplySorting(
            IList<ProductCategoryModel> source,
            QueryParameters<ProductCategoryModel> parameters)
        {
            if (parameters.Sorting is null || !parameters.Sorting.Any())
                return source;

            var sortingItems = parameters.Sorting
                .Select(QueryParameters<ProductCategoryModel>.FromModel)
                .ToList();

            return source.AsQueryable()
                         .ApplySorting(sortingItems)!
                         .ToList();
        }
    }
}