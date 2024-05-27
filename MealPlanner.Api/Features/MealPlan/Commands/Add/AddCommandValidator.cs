using FluentValidation;

namespace MealPlanner.Api.Features.MealPlan.Commands.Add
{
    public class AddCommandValidator : AbstractValidator<AddCommand>
    {
        public AddCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
