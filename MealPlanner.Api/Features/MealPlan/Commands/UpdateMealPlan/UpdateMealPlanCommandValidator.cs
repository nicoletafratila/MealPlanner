using FluentValidation;

namespace MealPlanner.Api.Features.MealPlan.Commands.UpdateMealPlan
{
    public class UpdateMealPlanCommandValidator : AbstractValidator<UpdateMealPlanCommand>
    {
        public UpdateMealPlanCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
