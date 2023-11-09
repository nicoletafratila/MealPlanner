using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Product.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, UpdateProductCommandResponse>
    {
        private readonly IProductRepository _repository;

        public UpdateProductCommandHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<UpdateProductCommandResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var oldModel = await _repository.GetByIdAsync(request.Id);
            if (oldModel == null)
            {
                return new UpdateProductCommandResponse { Message = $"Could not find with id {request.Id}" };
            }

            oldModel.Name= request.Name;
            oldModel.ImageContent= request.ImageContent;
            oldModel.UnitId= request.UnitId;
            oldModel.ProductCategoryId= request.ProductCategoryId;
            await _repository.UpdateAsync(oldModel);
            
            return new UpdateProductCommandResponse();
        }
    }
}
