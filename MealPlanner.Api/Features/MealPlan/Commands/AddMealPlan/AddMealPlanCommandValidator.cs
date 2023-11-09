using FluentValidation;

namespace MealPlanner.Api.Features.MealPlan.Commands.AddMealPlan
{
    public class AddMealPlanCommandValidator : AbstractValidator<AddMealPlanCommand>
    {
        public AddMealPlanCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
