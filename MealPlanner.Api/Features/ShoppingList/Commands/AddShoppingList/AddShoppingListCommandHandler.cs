using AutoMapper;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.AddShoppingList
{
    public class AddShoppingListCommandHandler(IShoppingListRepository repository, IMapper mapper, ILogger<AddShoppingListCommandHandler> logger) : IRequestHandler<AddShoppingListCommand, AddShoppingListCommandResponse>
    {
        private readonly IShoppingListRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<AddShoppingListCommandHandler> _logger = logger;

        public async Task<AddShoppingListCommandResponse> Handle(AddShoppingListCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingItem = await _repository.SearchAsync(request.Model?.Name!);
                if (existingItem != null)
                    return new AddShoppingListCommandResponse { Id = 0, Message = "This shopping list already exists." };

                var mapped = _mapper.Map<Common.Data.Entities.ShoppingList>(request.Model);
                var newItem = await _repository.AddAsync(mapped);
                return new AddShoppingListCommandResponse { Id = newItem.Id, Message = string.Empty };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new AddShoppingListCommandResponse { Message = "An error occured when saving the shopping list." };
            }
        }
    }
}
