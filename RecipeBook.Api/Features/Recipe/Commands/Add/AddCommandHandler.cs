using AutoMapper;
using Common.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Recipe.Commands.Add
{
    /// <summary>
    /// Handles adding a new recipe.
    /// </summary>
    public class AddCommandHandler(
        IRecipeRepository repository,
        IMapper mapper,
        ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, CommandResponse?>
    {
        private readonly IRecipeRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<AddCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null)
                throw new ArgumentNullException(nameof(request), "Model cannot be null.");

            try
            {
                var existingItem = await _repository.SearchAsync(request.Model.Name!);
                if (existingItem is not null)
                {
                    return CommandResponse.Failed("This recipe already exists in this category.");
                }

                var mapped = _mapper.Map<Common.Data.Entities.Recipe>(request.Model);
                await _repository.AddAsync(mapped);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when saving the recipe with name {Name}.", request.Model.Name);
                return CommandResponse.Failed("An error occurred when saving the recipe.");
            }
        }
    }
}