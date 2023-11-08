using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Product.Commands.AddProduct
{
    public class AddProductCommandHandler : IRequestHandler<AddProductCommand, AddProductCommandResponse>
    {
        private readonly IProductRepository _repository;

        public AddProductCommandHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<AddProductCommandResponse> Handle(AddProductCommand request, CancellationToken cancellationToken)
        {
            var existingItem = await _repository.SearchAsync(request.Name!);
            if (existingItem != null)
                return new AddProductCommandResponse { Id = 0, Message = "This product already exists." };

            Common.Data.Entities.Product newItem = new()
            {
                Name = request.Name,
                ImageContent = request.ImageContent,
                UnitId = request.UnitId,
                ProductCategoryId = request.ProductCategoryId
            };
            newItem = await _repository.AddAsync(newItem);
            return new AddProductCommandResponse { Id = newItem.Id, Message = string.Empty };
        }
    }
}
