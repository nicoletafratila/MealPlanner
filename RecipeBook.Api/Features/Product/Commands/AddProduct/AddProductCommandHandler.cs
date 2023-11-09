using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Product.Commands.AddProduct
{
    public class AddProductCommandHandler : IRequestHandler<AddProductCommand, AddProductCommandResponse>
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<AddProductCommandHandler> _logger;

        public AddProductCommandHandler(IProductRepository repository, IMapper mapper, ILogger<AddProductCommandHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AddProductCommandResponse> Handle(AddProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingItem = await _repository.SearchAsync(request.Model!.Name!);
                if (existingItem != null)
                    return new AddProductCommandResponse { Id = 0, Message = "This product already exists." };

                var mapped = _mapper.Map<Common.Data.Entities.Product>(request.Model);
                var newItem = await _repository.AddAsync(mapped);
                return new AddProductCommandResponse { Id = newItem.Id, Message = string.Empty };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new AddProductCommandResponse { Message = "An error occured when saving the product." };
            }
        }
    }
}
