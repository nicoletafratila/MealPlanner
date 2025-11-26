using AutoMapper;
using Common.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Product.Commands.Add
{
    public class AddCommandHandler(IProductRepository repository, IMapper mapper, ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, CommandResponse?>
    {
        public async Task<CommandResponse?> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingItem = await repository.SearchAsync(request.Model?.Name!);
                if (existingItem != null)
                    return CommandResponse.Failed("This product already exists.");

                var mapped = mapper.Map<Common.Data.Entities.Product>(request.Model);
                var newItem = await repository.AddAsync(mapped);
                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return CommandResponse.Failed("An error occurred when saving the product.");
            }
        }
    }
}
