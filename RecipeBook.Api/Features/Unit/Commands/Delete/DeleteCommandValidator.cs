using FluentValidation;

namespace RecipeBook.Api.Features.Unit.Commands.Delete
{
    /// <summary>
    /// Validates delete commands for units.
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