using AutoMapper;
using Common.Models;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Commands.Add
{
    public class AddCommandHandler(IMealPlanRepository repository, IMapper mapper, ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, CommandResponse>
    {
        public async Task<CommandResponse> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingItem = await repository.SearchAsync(request.Model?.Name!);
                if (existingItem != null)
                    return CommandResponse.Failed("This meal plan already exists.");

                var mapped = mapper.Map<Common.Data.Entities.MealPlan>(request.Model);
                var newItem = await repository.AddAsync(mapped);
                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return CommandResponse.Failed("An error occurred when saving the meal plan.");
            }
        }
    }
}
