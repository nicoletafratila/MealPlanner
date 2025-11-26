using AutoMapper;
using Common.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Unit.Commands.Add
{
    public class AddCommandHandler(IUnitRepository repository, IMapper mapper, ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, CommandResponse?>
    {
        public async Task<CommandResponse?> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var Units = await repository.GetAllAsync();
                var existingItem = Units?.FirstOrDefault(i => i.Name == request.Model?.Name!);
                if (existingItem != null)
                    return CommandResponse.Failed("This product category already exists.");

                var mapped = mapper.Map<Common.Data.Entities.Unit>(request.Model);
                var newItem = await repository.AddAsync(mapped);
                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return CommandResponse.Failed("An error occurred when saving the product category.");
            }
        }
    }
}
