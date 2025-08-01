using AutoMapper;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Commands.Add
{
    public class AddCommandHandler(IMealPlanRepository repository, IMapper mapper, ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, AddCommandResponse>
    {
        private readonly IMealPlanRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<AddCommandHandler> _logger = logger;

        public async Task<AddCommandResponse> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingItem = await _repository.SearchAsync(request.Model?.Name!);
                if (existingItem != null)
                    return new AddCommandResponse { Id = 0, Message = "This meal plan already exists." };

                var mapped = _mapper.Map<Common.Data.Entities.MealPlan>(request.Model);
                var newItem = await _repository.AddAsync(mapped);
                return new AddCommandResponse { Id = newItem.Id, Message = string.Empty };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new AddCommandResponse { Message = "An error occurred when saving the meal plan." };
            }
        }
    }
}
