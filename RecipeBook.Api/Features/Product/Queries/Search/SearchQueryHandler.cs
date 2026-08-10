using AutoMapper;
using Common.Pagination;
using Common.Services;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.Search
{
    public class SearchQueryHandler(IProductRepository repository, IMapper mapper, ICurrentUserService currentUserService) : IRequestHandler<SearchQuery, PagedList<ProductModel>>
    {
        private readonly IProductRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

        public async Task<PagedList<ProductModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            if (request?.QueryParameters is null)
            {
                return new([], new Metadata());
            }

            var qp = request.QueryParameters;
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return new([], new Metadata());

            Guid? categoryId = null;
            if (!string.IsNullOrWhiteSpace(request.CategoryId))
            {
                // Malformed category ids never match any product, matching the previous DTO-string-comparison behavior.
                if (!Guid.TryParse(request.CategoryId, out var parsedCategoryId))
                    return new([], Metadata.Create(qp.PageNumber, qp.PageSize, 0));

                categoryId = parsedCategoryId;
            }

            var (entities, totalCount, skip) = await _repository.SearchByUserAsync(
                userId, categoryId, qp.Filters, qp.Sorting, qp.PageNumber, qp.PageSize, cancellationToken, request.ThumbnailOnly);

            var models = _mapper.Map<IList<ProductModel>>(entities) ?? [];
            var metadata = Metadata.Create(qp.PageNumber, qp.PageSize, totalCount);
            for (var i = 0; i < models.Count; i++)
                models[i].Index = skip + i + 1;

            return new PagedList<ProductModel>(models, metadata);
        }
    }
}
