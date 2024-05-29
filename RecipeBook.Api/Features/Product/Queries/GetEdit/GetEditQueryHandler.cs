using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.GetEdit
{
    public class GetEditQueryHandler(IProductRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, ProductEditModel>
    {
        private readonly IProductRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<ProductEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdAsync(request.Id);
            return _mapper.Map<ProductEditModel>(result);
        }
    }
}
