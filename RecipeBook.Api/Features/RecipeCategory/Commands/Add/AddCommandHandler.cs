using AutoMapper;
using Common.Models;
using Common.Services;
using MediatR;
using RecipeBook.Api.Features.RecipeCategory.Resources;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Add
{
    /// <summary>
    /// Handles adding new recipe categories.
    /// </summary>
    public class AddCommandHandler(
        IRecipeCategoryRepository repository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, CommandResponse?>
    {
        private readonly IRecipeCategoryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        private readonly ILogger<AddCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null)
                throw new ArgumentNullException(nameof(request), RecipeCategoryMessages.ModelCannotBeNull);

            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                    return CommandResponse.Failed(RecipeCategoryMessages.UserIdRequired);

                var allCategories = await _repository.GetAllByUserAsync(userId, cancellationToken) ?? [];

                var newName = request.Model.Name?.Trim();
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    var exists = allCategories.Any(c =>
                        !string.IsNullOrWhiteSpace(c.Name) &&
                        c.Name.Trim().Equals(newName, StringComparison.OrdinalIgnoreCase));

                    if (exists)
                    {
                        return CommandResponse.Failed(RecipeCategoryMessages.RecipeCategoryAlreadyExists);
                    }
                }

                var mapped = _mapper.Map<RecipeBook.Data.Entities.RecipeCategory>(request.Model);
                mapped.Id = Guid.NewGuid();
                mapped.UserId = userId;
                await _repository.AddAsync(mapped, cancellationToken);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when saving the recipe category.");
                return CommandResponse.Failed(RecipeCategoryMessages.SaveFailed);
            }
        }
    }
}