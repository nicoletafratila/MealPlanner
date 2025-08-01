using AutoMapper;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.Add
{
    public class AddCommandHandler(IShopRepository repository, IMapper mapper, ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, AddCommandResponse>
    {
        private readonly IShopRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<AddCommandHandler> _logger = logger;

        public async Task<AddCommandResponse> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var shops = await _repository.GetAllAsync();
                var existingItem = shops?.FirstOrDefault(i => i.Name == request.Model?.Name!);
                if (existingItem != null)
                    return new AddCommandResponse { Id = 0, Message = "This shop already exists." };

                var mapped = _mapper.Map<Common.Data.Entities.Shop>(request.Model);
                var newItem = await _repository.AddAsync(mapped);
                return new AddCommandResponse { Id = newItem.Id, Message = string.Empty };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new AddCommandResponse { Message = "An error occurred when saving the shop." };
            }
        }
    }
}
