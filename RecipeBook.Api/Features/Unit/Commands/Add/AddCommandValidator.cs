using FluentValidation;
using RecipeBook.Api.Features.Unit.Resources;

namespace RecipeBook.Api.Features.Unit.Commands.Add
{
    /// <summary>
    /// Validates add commands for units.
    /// </summary>
    public class AddCommandValidator : AbstractValidator<AddCommand>
    {
        public AddCommandValidator()
        {
            RuleFor(x => x.Model)
                .NotNull()
                .WithMessage(UnitMessages.ModelRequired);
        }
    }
}