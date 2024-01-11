﻿using AutoMapper;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.UpdateShop
{
    public class UpdateShopCommandHandler(IShopRepository repository, IMapper mapper, ILogger<UpdateShopCommandHandler> logger) : IRequestHandler<UpdateShopCommand, UpdateShopCommandResponse>
    {
        private readonly IShopRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<UpdateShopCommandHandler> _logger = logger;

        public async Task<UpdateShopCommandResponse> Handle(UpdateShopCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingItem = await _repository.GetByIdAsync(request.Model!.Id);
                if (existingItem == null)
                    return new UpdateShopCommandResponse { Message = $"Could not find with id {request.Model!.Id}" };

                _mapper.Map(request.Model, existingItem);
                await _repository.UpdateAsync(existingItem);
                return new UpdateShopCommandResponse();
            }
            catch (Exception ex)
            {
#pragma warning disable CA2254 // Template should be a static expression
                _logger.LogError(ex.Message, ex);
#pragma warning restore CA2254 // Template should be a static expression
                return new UpdateShopCommandResponse { Message = "An error occured when saving the shop." };
            }
        }
    }
}
