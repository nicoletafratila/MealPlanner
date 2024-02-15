using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.GetEditProduct
{
    public class GetEditProductQueryHandler(IProductRepository repository, IMapper mapper) : IRequestHandler<GetEditProductQuery, EditProductModel>
    {
        private readonly IProductRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<EditProductModel> Handle(GetEditProductQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdAsync(request.Id);
            return _mapper.Map<EditProductModel>(result);
        }
    }
}
