using FluentValidation;

namespace MealPlanner.Api.Features.MealPlan.Commands.Update
{
    /// <summary>
    /// Validates meal-plan update commands.
    /// </summary>
    public class UpdateCommandValidator : AbstractValidator<UpdateCommand>
    {
        public UpdateCommandValidator()
        {
            RuleFor(x => x.Model)
                .NotNull()
                .WithMessage("Model is required.");
        }
    }
}