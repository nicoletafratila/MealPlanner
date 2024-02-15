using AutoMapper;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Commands.UpdateMealPlan
{
    public class UpdateMealPlanCommandHandler(IMealPlanRepository repository, IMapper mapper, ILogger<UpdateMealPlanCommandHandler> logger) : IRequestHandler<UpdateMealPlanCommand, UpdateMealPlanCommandResponse>
    {
        private readonly IMealPlanRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<UpdateMealPlanCommandHandler> _logger = logger;

        public async Task<UpdateMealPlanCommandResponse> Handle(UpdateMealPlanCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingItem = await _repository.GetByIdAsync(request.Model!.Id);
                if (existingItem == null)
                    return new UpdateMealPlanCommandResponse { Message = $"Could not find with id {request.Model?.Id}" };

                _mapper.Map(request.Model, existingItem);
                await _repository.UpdateAsync(existingItem);
                return new UpdateMealPlanCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new UpdateMealPlanCommandResponse { Message = "An error occured when saving the meal plan." };
            }
        }
    }
}
