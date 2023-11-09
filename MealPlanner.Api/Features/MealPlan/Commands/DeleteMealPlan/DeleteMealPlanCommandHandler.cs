using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Commands.DeleteMealPlan
{
    public class DeleteMealPlanCommandHandler : IRequestHandler<DeleteMealPlanCommand, DeleteMealPlanCommandResponse>
    {
        private readonly IMealPlanRepository _repository;
        private readonly ILogger<DeleteMealPlanCommandHandler> _logger;

        public DeleteMealPlanCommandHandler(IMealPlanRepository repository, ILogger<DeleteMealPlanCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<DeleteMealPlanCommandResponse> Handle(DeleteMealPlanCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var itemToDelete = await _repository.GetByIdAsync(request.Id);
                if (itemToDelete == null)
                {
                    return new DeleteMealPlanCommandResponse { Message = $"Could not find with id {request.Id}" };
                }

                await _repository.DeleteAsync(itemToDelete!);
                return new DeleteMealPlanCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new DeleteMealPlanCommandResponse { Message = "An error occured when deleting the meal plan." };
            }
        }
    }
}
