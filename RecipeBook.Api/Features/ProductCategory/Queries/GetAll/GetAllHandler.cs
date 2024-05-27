using AutoMapper;
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
            var results = await _repository.GetAllAsync();
            return _mapper.Map<IList<ProductCategoryModel>>(results).OrderBy(r => r.Name).ToList();
        }
    }
}
