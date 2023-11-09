using FluentValidation;

namespace MealPlanner.Api.Features.MealPlan.Commands.DeleteMealPlan
{
    public class DeleteMealPlanCommandValidator : AbstractValidator<DeleteMealPlanCommand>
    {
        public DeleteMealPlanCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
