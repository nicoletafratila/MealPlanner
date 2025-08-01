using AutoMapper;
using Common.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Add
{
    public class AddCommandHandler(IRecipeCategoryRepository repository, IMapper mapper, ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, CommandResponse>
    {
        public async Task<CommandResponse> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var RecipeCategorys = await repository.GetAllAsync();
                var existingItem = RecipeCategorys?.FirstOrDefault(i => i.Name == request.Model?.Name!);
                if (existingItem != null)
                    return CommandResponse.Failed("This Recipe category already exists.");

                var mapped = mapper.Map<Common.Data.Entities.RecipeCategory>(request.Model);
                var newItem = await repository.AddAsync(mapped);
                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return CommandResponse.Failed("An error occurred when saving the Recipe category.");
            }
        }
    }
}
