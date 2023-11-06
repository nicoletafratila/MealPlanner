using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.GetProducts
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IList<ProductModel>>
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;

        public GetProductsQueryHandler(IProductRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IList<ProductModel>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var results = await _repository.GetAllAsync();
            return _mapper.Map<IList<ProductModel>>(results).OrderBy(item => item.ProductCategory!.DisplaySequence).ThenBy(item => item.Name).ToList();
        }
    }
}
