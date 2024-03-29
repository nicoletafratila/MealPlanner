﻿using AutoMapper;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.UpdateShoppingList
{
    public class UpdateShoppingListCommandHandler(IShoppingListRepository repository, IMapper mapper, ILogger<UpdateShoppingListCommandHandler> logger) : IRequestHandler<UpdateShoppingListCommand, UpdateShoppingListCommandResponse>
    {
        private readonly IShoppingListRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<UpdateShoppingListCommandHandler> _logger = logger;

        public async Task<UpdateShoppingListCommandResponse> Handle(UpdateShoppingListCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingItem = await _repository.GetByIdIncludeProductsAsync(request.Model!.Id);
                if (existingItem == null)
                    return new UpdateShoppingListCommandResponse { Message = $"Could not find with id {request.Model?.Id}" };

                _mapper.Map(request.Model, existingItem);
                await _repository.UpdateAsync(existingItem);
                return new UpdateShoppingListCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new UpdateShoppingListCommandResponse { Message = "An error occured when saving the meal plan." };
            }
        }
    }
}
