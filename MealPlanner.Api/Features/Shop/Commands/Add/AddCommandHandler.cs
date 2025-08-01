using AutoMapper;
using Common.Models;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.Add
{
    public class AddCommandHandler(IShopRepository repository, IMapper mapper, ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, CommandResponse>
    {
        public async Task<CommandResponse> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var shops = await repository.GetAllAsync();
                var existingItem = shops?.FirstOrDefault(i => i.Name == request.Model?.Name!);
                if (existingItem != null)
                    return CommandResponse.Failed("This shop already exists.");

                var mapped = mapper.Map<Common.Data.Entities.Shop>(request.Model);
                var newItem = await repository.AddAsync(mapped);
                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return CommandResponse.Failed("An error occurred when saving the shop.");
            }
        }
    }
}
