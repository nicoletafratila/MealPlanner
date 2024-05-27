using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Commands.Delete
{
    public class DeleteCommandHandler(IMealPlanRepository repository, ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, DeleteCommandResponse>
    {
        private readonly IMealPlanRepository _repository = repository;
        private readonly ILogger<DeleteCommandHandler> _logger = logger;

        public async Task<DeleteCommandResponse> Handle(DeleteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var itemToDelete = await _repository.GetByIdAsync(request.Id);
                if (itemToDelete == null)
                {
                    return new DeleteCommandResponse { Message = $"Could not find with id {request.Id}" };
                }

                await _repository.DeleteAsync(itemToDelete!);
                return new DeleteCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new DeleteCommandResponse { Message = "An error occured when deleting the meal plan." };
            }
        }
    }
}
