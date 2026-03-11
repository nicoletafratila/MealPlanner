using AutoMapper;
using Common.Models;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.Update
{
    /// <summary>
    /// Handles updating a shop (including its display sequence).
    /// </summary>
    public class UpdateCommandHandler(
        IShopRepository repository,
        IMapper mapper,
        ILogger<UpdateCommandHandler> logger) : IRequestHandler<UpdateCommand, CommandResponse?>
    {
        private readonly IShopRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<UpdateCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(UpdateCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null)
                throw new ArgumentNullException(nameof(request), "Model cannot be null.");

            try
            {
                var existingItem = await _repository.GetByIdIncludeDisplaySequenceAsync(request.Model.Id, cancellationToken);
                if (existingItem is null)
                {
                    return CommandResponse.Failed($"Could not find with id {request.Model.Id}");
                }

                _mapper.Map(request.Model, existingItem);
                await _repository.UpdateAsync(existingItem, cancellationToken);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred when saving the shop with id {Id}.",
                    request.Model.Id);

                return CommandResponse.Failed("An error occurred when saving the shop.");
            }
        }
    }
}