using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.GetEdit
{
    /// <summary>
    /// Handles retrieving a product for editing.
    /// </summary>
    public class GetEditQueryHandler(IProductRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, ProductEditModel>
    {
        private readonly IProductRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<ProductEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (entity is null)
            {
                return new ProductEditModel { Id = request.Id };
            }

            var model = _mapper.Map<ProductEditModel>(entity);
            return model ?? new ProductEditModel { Id = request.Id };
        }
    }
}