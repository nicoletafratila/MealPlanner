using AutoMapper;
using Common.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.ApplicationUser.Commands.Update
{
    /// <summary>
    /// Handles updating an existing application user.
    /// </summary>
    public class UpdateCommandHandler(
        UserManager<Common.Data.Entities.ApplicationUser> userManager,
        IMapper mapper,
        ILogger<UpdateCommandHandler> logger) : IRequestHandler<UpdateCommand, CommandResponse?>
    {
        private readonly UserManager<Common.Data.Entities.ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<UpdateCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(UpdateCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null || string.IsNullOrWhiteSpace(request.Model.UserId))
                return CommandResponse.Failed($"Could not find a user with id = {request?.Model?.UserId}");

            try
            {
                var existingItem = await _userManager.FindByIdAsync(request.Model.UserId);
                if (existingItem is null)
                    return CommandResponse.Failed($"Could not find a user with id = {request.Model.UserId}");

                _mapper.Map(request.Model, existingItem);

                var updateResult = await _userManager.UpdateAsync(existingItem);
                if (!updateResult.Succeeded)
                {
                    var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to update user {UserId}: {Errors}", request.Model.UserId, errors);
                    return CommandResponse.Failed(errors);
                }

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the user {UserId}", request.Model.UserId);
                return CommandResponse.Failed("An error occurred while updating the user.");
            }
        }
    }
}