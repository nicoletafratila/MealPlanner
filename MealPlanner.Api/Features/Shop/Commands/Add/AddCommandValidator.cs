using FluentValidation;

namespace MealPlanner.Api.Features.Shop.Commands.Add
{
    public class AddCommandValidator : AbstractValidator<AddCommand>
    {
        public AddCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
