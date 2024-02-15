using AutoMapper;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Commands.AddMealPlan
{
    public class AddMealPlanCommandHandler(IMealPlanRepository repository, IMapper mapper, ILogger<AddMealPlanCommandHandler> logger) : IRequestHandler<AddMealPlanCommand, AddMealPlanCommandResponse>
    {
        private readonly IMealPlanRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<AddMealPlanCommandHandler> _logger = logger;

        public async Task<AddMealPlanCommandResponse> Handle(AddMealPlanCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingItem = await _repository.SearchAsync(request.Model?.Name!);
                if (existingItem != null)
                    return new AddMealPlanCommandResponse { Id = 0, Message = "This meal plan already exists." };

                var mapped = _mapper.Map<Common.Data.Entities.MealPlan>(request.Model);
                var newItem = await _repository.AddAsync(mapped);
                return new AddMealPlanCommandResponse { Id = newItem.Id, Message = string.Empty };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new AddMealPlanCommandResponse { Message = "An error occured when saving the meal plan." };
            }
        }
    }
}
