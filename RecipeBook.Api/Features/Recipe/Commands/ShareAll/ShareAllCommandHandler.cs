using Common.Models;
using Common.Services;
using MediatR;
using RecipeBook.Api.Features.Recipe.Commands.Share;
using RecipeBook.Api.Features.Recipe.Resources;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Recipe.Commands.ShareAll
{
    /// <summary>
    /// Handles sharing all of the current user's recipes with another user, reusing <see cref="ShareCommand"/> for each recipe.
    /// </summary>
    public class ShareAllCommandHandler(
        IRecipeRepository repository,
        ICurrentUserService currentUserService,
        ISender mediator,
        ILogger<ShareAllCommandHandler> logger) : IRequestHandler<ShareAllCommand, CommandResponse?>
    {
        private readonly IRecipeRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        private readonly ISender _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        private readonly ILogger<ShareAllCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(ShareAllCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                    return CommandResponse.Failed(RecipeMessages.UserIdRequired);

                var recipes = await _repository.GetAllByUserAsync(userId, cancellationToken);

                foreach (var recipe in recipes)
                {
                    var shareCommand = new ShareCommand { RecipeId = recipe.Id, TargetUserId = request.TargetUserId };
                    var response = await _mediator.Send(shareCommand, cancellationToken);

                    if (response is null || !response.Succeeded)
                    {
                        return CommandResponse.Failed(response?.Message ?? RecipeMessages.SaveFailed);
                    }
                }

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when sharing all recipes to user {TargetUserId}.", request.TargetUserId);
                return CommandResponse.Failed(RecipeMessages.SaveFailed);
            }
        }
    }
}
