using AutoMapper;
using Common.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.ApplicationUser.Commands.Update
{
    public class UpdateCommandHandler(UserManager<Common.Data.Entities.ApplicationUser> userManager, IMapper mapper, ILogger<UpdateCommandHandler> logger) : IRequestHandler<UpdateCommand, CommandResponse?>
    {
        public async Task<CommandResponse?> Handle(UpdateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request?.Model is null || string.IsNullOrWhiteSpace(request.Model.UserId))
                    return CommandResponse.Failed($"Could not find a user with id = {request?.Model?.UserId}");

                var existingItem = await userManager.FindByIdAsync(request.Model.UserId);
                if (existingItem is null)
                    return CommandResponse.Failed($"Could not find a user with id = {request.Model.UserId}");

                mapper.Map(request.Model, existingItem);

                var updateResult = await userManager.UpdateAsync(existingItem);
                if (!updateResult.Succeeded)
                {
                    var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                    logger.LogWarning("Failed to update user {UserId}: {Errors}", request.Model.UserId, errors);
                    return CommandResponse.Failed(errors);
                }

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while updating the user {UserId}", request?.Model?.UserId);
                return CommandResponse.Failed("An error occurred while updating the user.");
            }
        }
    }
}
