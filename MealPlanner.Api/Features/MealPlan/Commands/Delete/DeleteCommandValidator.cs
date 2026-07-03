using FluentValidation;

namespace MealPlanner.Api.Features.MealPlan.Commands.Delete
{
    /// <summary>
    /// Validates DeleteCommand for meal plans.
    /// </summary>
    public class DeleteCommandValidator : AbstractValidator<DeleteCommand>
    {
        public DeleteCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty)
                .WithMessage(Resources.MealPlanMessages.IdGreaterThanZero);
        }
    }
}