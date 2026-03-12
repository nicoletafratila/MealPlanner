using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetEdit
{
    /// <summary>
    /// Handles retrieving a meal plan for editing.
    /// </summary>
    public class GetEditQueryHandler(IMealPlanRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, MealPlanEditModel>
    {
        private readonly IMealPlanRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<MealPlanEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (entity is null)
            {
                return new MealPlanEditModel { Id = request.Id };
            }

            var model = _mapper.Map<MealPlanEditModel>(entity);
            return model ?? new MealPlanEditModel { Id = request.Id };
        }
    }
}