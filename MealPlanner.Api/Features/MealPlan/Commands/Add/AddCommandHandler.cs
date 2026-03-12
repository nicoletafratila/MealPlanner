using AutoMapper;
using Common.Models;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Commands.Add
{
    /// <summary>
    /// Handles adding a new meal plan.
    /// </summary>
    public class AddCommandHandler(
        IMealPlanRepository repository,
        IMapper mapper,
        ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, CommandResponse?>
    {
        private readonly IMealPlanRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<AddCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null)
                throw new ArgumentNullException(nameof(request), "Model cannot be null.");

            try
            {
                var name = request.Model.Name ?? string.Empty;

                var existingItem = await _repository.SearchAsync(name, cancellationToken);
                if (existingItem is not null)
                {
                    return CommandResponse.Failed("This meal plan already exists.");
                }

                var mapped = _mapper.Map<Common.Data.Entities.MealPlan>(request.Model);
                await _repository.AddAsync(mapped, cancellationToken);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred when saving the meal plan '{Name}'.",
                    request.Model.Name);

                return CommandResponse.Failed("An error occurred when saving the meal plan.");
            }
        }
    }
}