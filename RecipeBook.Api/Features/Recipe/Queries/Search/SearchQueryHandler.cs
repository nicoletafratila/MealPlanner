using AutoMapper;
using Common.Pagination;
using Common.Services;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.Search
{
    public class SearchQueryHandler(IRecipeRepository repository, IMapper mapper, ICurrentUserService currentUserService) : IRequestHandler<SearchQuery, PagedList<RecipeModel>>
    {
        private readonly IRecipeRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

        public async Task<PagedList<RecipeModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            if (request?.QueryParameters is null)
            {
                return new([], new Metadata());
            }

            var qp = request.QueryParameters;
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return new([], new Metadata());

            // Malformed category ids are ignored (filter not applied), matching the previous behavior.
            Guid? categoryId = !string.IsNullOrWhiteSpace(request.CategoryId) && Guid.TryParse(request.CategoryId, out var parsedCategoryId)
                ? parsedCategoryId
                : null;

            var (entities, totalCount, skip) = await _repository.SearchByUserAsync(
                userId, categoryId, qp.Filters, qp.Sorting, qp.PageNumber, qp.PageSize, cancellationToken, request.ThumbnailOnly);

            var models = _mapper.Map<IList<RecipeModel>>(entities) ?? [];
            var metadata = Metadata.Create(qp.PageNumber, qp.PageSize, totalCount);
            for (var i = 0; i < models.Count; i++)
                models[i].Index = skip + i + 1;

            return new PagedList<RecipeModel>(models, metadata);
        }
    }
}
