using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.GetEditProduct
{
    public class GetEditProductQueryHandler : IRequestHandler<GetEditProductQuery, EditProductModel>
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;

        public GetEditProductQueryHandler(IProductRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<EditProductModel> Handle(GetEditProductQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdAsync(request.Id);
            return _mapper.Map<EditProductModel>(result);
        }
    }
}
