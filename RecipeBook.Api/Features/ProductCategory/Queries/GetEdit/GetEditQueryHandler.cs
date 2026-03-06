using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.GetEdit
{
    /// <summary>
    /// Handles retrieving a product category for editing.
    /// </summary>
    public class GetEditQueryHandler(IProductCategoryRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, ProductCategoryEditModel>
    {
        private readonly IProductCategoryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<ProductCategoryEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _repository.GetByIdAsync(request.Id);

            if (entity is null)
            {
                return new ProductCategoryEditModel { Id = request.Id };
            }

            var model = _mapper.Map<ProductCategoryEditModel>(entity);
            return model ?? new ProductCategoryEditModel { Id = request.Id };
        }
    }
}