using AutoMapper;
using Common.Models;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.GetAll
{
    public class GetAllHandler(IProductCategoryRepository repository, IMapper mapper) : IRequestHandler<GetAllQuery, IList<ProductCategoryModel>>
    {
        private readonly IProductCategoryRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<IList<ProductCategoryModel>> Handle(GetAllQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetAllAsync();
            var results = _mapper.Map<IList<ProductCategoryModel>>(data).OrderBy(r => r.Name).ToList();
            results.SetIndexes();
            return results;
        }
    }
}
