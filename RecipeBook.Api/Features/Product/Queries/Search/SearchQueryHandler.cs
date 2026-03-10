using AutoMapper;
using Common.Pagination;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.Search
{
    public class SearchQueryHandler(IProductRepository repository, IMapper mapper) : IRequestHandler<SearchQuery, PagedList<ProductModel>>
    {
        private readonly IProductRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<PagedList<ProductModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            if (request?.QueryParameters is null)
            {
                return new([], new Metadata());
            }

            var qp = request.QueryParameters;

            var entities = await _repository.GetAllAsync(cancellationToken);
            var models = _mapper.Map<IList<ProductModel>>(entities) ?? [];

            models = ApplyCategoryFilter(models, request.CategoryId);
            models = ApplyFilters(models, qp);
            models = ApplySorting(models, qp);

            return models.ToPagedList(qp.PageNumber, qp.PageSize);
        }

        private static IList<ProductModel> ApplyCategoryFilter(
            IList<ProductModel> source,
            string? categoryId)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
                return source;

            return source.Where(p => p.ProductCategoryId == categoryId).ToList();
        }

        private static IList<ProductModel> ApplyFilters(
            IList<ProductModel> source,
            QueryParameters<ProductModel> parameters)
        {
            if (parameters.Filters is null || !parameters.Filters.Any())
                return source;

            var result = source;

            foreach (var filter in parameters.Filters)
            {
                var predicate = filter.ConvertFilterItemToFunc<ProductModel>();
                result = result.Where(predicate).ToList();
            }

            return result;
        }

        private static IList<ProductModel> ApplySorting(
            IList<ProductModel> source,
            QueryParameters<ProductModel> parameters)
        {
            if (parameters.Sorting is null || !parameters.Sorting.Any())
                return source;

            var sortingItems = parameters.Sorting
                .Select(QueryParameters<ProductModel>.FromModel)
                .ToList();

            return source.AsQueryable()
                         .ApplySorting(sortingItems)!
                         .ToList();
        }
    }
}