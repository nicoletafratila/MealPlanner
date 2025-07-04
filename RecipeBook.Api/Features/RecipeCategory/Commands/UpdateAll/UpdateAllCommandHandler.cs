using System.Text;
using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.UpdateAll
{
    public class UpdateAllCommandHandler(IRecipeCategoryRepository repository, IMapper mapper, ILogger<UpdateAllCommandHandler> logger) : IRequestHandler<UpdateAllCommand, UpdateAllCommandResponse>
    {
        private readonly IRecipeCategoryRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<UpdateAllCommandHandler> _logger = logger;

        public async Task<UpdateAllCommandResponse> Handle(UpdateAllCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = new StringBuilder();
                var itemsToUpdate = new List<Common.Data.Entities.RecipeCategory>();
                foreach (var category in request.Models!)
                {
                    var existingItem = await _repository.GetByIdAsync(category.Id);
                    if (existingItem == null)
                        result.AppendLine($"Could not find with id {category.Id}");

                    _mapper.Map(category, existingItem);
                    itemsToUpdate.Add(existingItem!);
                    //await _repository.UpdateAsync(existingItem!);
                }

                if (!string.IsNullOrWhiteSpace(result.ToString()))
                    return new UpdateAllCommandResponse(result.ToString());

                await _repository.UpdateAllAsync(itemsToUpdate!);
                return new UpdateAllCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new UpdateAllCommandResponse { Message = "An error occurred when saving the Recipe category." };
            }
        }
    }
}
