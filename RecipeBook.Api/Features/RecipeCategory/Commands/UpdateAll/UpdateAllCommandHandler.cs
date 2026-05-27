using System.Text;
using AutoMapper;
using Common.Models;
using MediatR;
using RecipeBook.Api.Features.RecipeCategory.Resources;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.UpdateAll
{
    /// <summary>
    /// Handles bulk update of recipe categories.
    /// </summary>
    public class UpdateAllCommandHandler(
        IRecipeCategoryRepository repository,
        IMapper mapper,
        ILogger<UpdateAllCommandHandler> logger) : IRequestHandler<UpdateAllCommand, CommandResponse?>
    {
        private readonly IRecipeCategoryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<UpdateAllCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(UpdateAllCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Models is null)
                throw new ArgumentNullException(nameof(request), "Model cannot be null.");

            try
            {
                var ids = request.Models
                    .Where(m => m is not null)
                    .Select(m => m!.Id)
                    .ToList();

                if (ids.Count == 0)
                    return CommandResponse.Success();

                var existingItems = await _repository.GetByIdsAsync(ids, cancellationToken);
                var existingById = existingItems.ToDictionary(e => e.Id);

                var errors = new StringBuilder();
                var itemsToUpdate = new List<RecipeBook.Data.Entities.RecipeCategory>();

                foreach (var category in request.Models)
                {
                    if (category is null)
                        continue;

                    if (!existingById.TryGetValue(category.Id, out var existingItem))
                    {
                        errors.AppendLine(string.Format(RecipeCategoryMessages.NotFoundById, category.Id));
                        continue;
                    }

                    _mapper.Map(category, existingItem);
                    itemsToUpdate.Add(existingItem);
                }

                if (errors.Length > 0)
                    return CommandResponse.Failed(errors.ToString().TrimEnd());

                if (itemsToUpdate.Count == 0)
                    return CommandResponse.Success();

                await _repository.UpdateAllAsync(itemsToUpdate, cancellationToken);
                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when saving the recipe categories.");
                return CommandResponse.Failed(RecipeCategoryMessages.SaveFailed);
            }
        }
    }
}