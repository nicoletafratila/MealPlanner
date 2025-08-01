using System.Text;
using AutoMapper;
using Common.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.UpdateAll
{
    public class UpdateAllCommandHandler(IRecipeCategoryRepository repository, IMapper mapper, ILogger<UpdateAllCommandHandler> logger) : IRequestHandler<UpdateAllCommand, CommandResponse>
    {
        public async Task<CommandResponse> Handle(UpdateAllCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = new StringBuilder();
                var itemsToUpdate = new List<Common.Data.Entities.RecipeCategory>();
                foreach (var category in request.Models!)
                {
                    var existingItem = await repository.GetByIdAsync(category.Id);
                    if (existingItem == null)
                        result.AppendLine($"Could not find with id {category.Id}");

                    mapper.Map(category, existingItem);
                    itemsToUpdate.Add(existingItem!);
                }

                if (!string.IsNullOrWhiteSpace(result.ToString()))
                    return CommandResponse.Failed(result.ToString());

                await repository.UpdateAllAsync(itemsToUpdate!);
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
