using AutoMapper;
using Common.Pagination;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.SearchRecipeCategory
{
    public class SearchRecipeCategoryQueryHandler : IRequestHandler<SearchRecipeCategoryQuery, PagedList<RecipeCategoryModel>>
    {
        private readonly IRecipeCategoryRepository _repository;
        private readonly IMapper _mapper;

        public SearchRecipeCategoryQueryHandler(IRecipeCategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PagedList<RecipeCategoryModel>> Handle(SearchRecipeCategoryQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetAllAsync();
            var results = _mapper.Map<IList<RecipeCategoryModel>>(data).OrderBy(item => item.Name);
            return results.ToPagedList(request.QueryParameters!.PageNumber, request.QueryParameters.PageSize);
        }
    }
}
