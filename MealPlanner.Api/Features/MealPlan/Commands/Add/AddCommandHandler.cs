using AutoMapper;
using Common.Models;
using Common.Services;
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
        ICurrentUserService currentUserService,
        ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, CommandResponse?>
    {
        private readonly IMealPlanRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        private readonly ILogger<AddCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null)
                throw new ArgumentNullException(nameof(request), Resources.MealPlanMessages.ModelCannotBeNull);

            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                    return CommandResponse.Failed(Resources.MealPlanMessages.UserIdRequired);

                var name = request.Model.Name ?? string.Empty;

                var existingItem = await _repository.SearchAsync(name, userId, cancellationToken);
                if (existingItem is not null)
                {
                    return CommandResponse.Failed(Resources.MealPlanMessages.AlreadyExists);
                }

                var mapped = _mapper.Map<MealPlanner.Data.Entities.MealPlan>(request.Model);
                mapped.UserId = userId;
                mapped.CreatedAt = DateTime.Now;
                await _repository.AddAsync(mapped, cancellationToken);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred when saving the meal plan '{Name}'.",
                    request.Model.Name);

                return CommandResponse.Failed(Resources.MealPlanMessages.SaveError);
            }
        }
    }
}