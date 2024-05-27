using AutoMapper;
using Common.Pagination;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.Search
{
    public class SearchQueryHandler(IProductRepository repository, IMapper mapper) : IRequestHandler<SearchQuery, PagedList<ProductModel>>
    {
        private readonly IProductRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<PagedList<ProductModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(request.CategoryId))
            {
                data = await _repository.SearchAsync(int.Parse(request.CategoryId));
            }
            var results = _mapper.Map<IList<ProductModel>>(data).OrderBy(item => item.ProductCategory?.Name).ThenBy(item => item.Name).ToList();
            return results.ToPagedList(request.QueryParameters!.PageNumber, request.QueryParameters.PageSize);
        }
    }
}
