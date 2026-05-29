using AutoMapper;
using Common.Pagination;
using Common.Services;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.Search
{
    public class SearchQueryHandler(IProductCategoryRepository repository, IMapper mapper, ICurrentUserService currentUserService) : IRequestHandler<SearchQuery, PagedList<ProductCategoryModel>>
    {
        private readonly IProductCategoryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

        public async Task<PagedList<ProductCategoryModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            if (request?.QueryParameters is null)
            {
                return new([], new Metadata());
            }

            var qp = request.QueryParameters;
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return new([], new Metadata());

            var entities = await _repository.GetAllByUserAsync(userId, cancellationToken);
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

            return source.AsQueryable()
                         .ApplySorting(parameters.Sorting)!
                         .ToList();
        }
    }
}