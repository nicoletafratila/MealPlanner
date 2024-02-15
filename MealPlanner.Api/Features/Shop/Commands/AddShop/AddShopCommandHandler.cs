using AutoMapper;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.AddShop
{
    public class AddShopCommandHandler(IShopRepository repository, IMapper mapper, ILogger<AddShopCommandHandler> logger) : IRequestHandler<AddShopCommand, AddShopCommandResponse>
    {
        private readonly IShopRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<AddShopCommandHandler> _logger = logger;

        public async Task<AddShopCommandResponse> Handle(AddShopCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var shops = await _repository.GetAllAsync();
                var existingItem = shops?.FirstOrDefault(i => i.Name == request.Model?.Name!);
                if (existingItem != null)
                    return new AddShopCommandResponse { Id = 0, Message = "This shop already exists." };

                var mapped = _mapper.Map<Common.Data.Entities.Shop>(request.Model);
                var newItem = await _repository.AddAsync(mapped);
                return new AddShopCommandResponse { Id = newItem.Id, Message = string.Empty };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new AddShopCommandResponse { Message = "An error occured when saving the shop." };
            }
        }
    }
}
