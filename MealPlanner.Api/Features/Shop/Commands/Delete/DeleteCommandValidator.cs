using FluentValidation;

namespace MealPlanner.Api.Features.Shop.Commands.Delete
{
    /// <summary>
    /// Validates DeleteCommand for shops.
    /// </summary>
    public class DeleteCommandValidator : AbstractValidator<DeleteCommand>
    {
        public DeleteCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Id must be greater than zero.");
        }
    }
}