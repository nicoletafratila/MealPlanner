using FluentValidation;
using RecipeBook.Api.Features.Unit.Resources;

namespace RecipeBook.Api.Features.Unit.Commands.Update
{
    /// <summary>
    /// Validates the update command for units.
    /// </summary>
    public class UpdateCommandValidator : AbstractValidator<UpdateCommand>
    {
        public UpdateCommandValidator()
        {
            RuleFor(x => x.Model)
                .NotNull()
                .WithMessage(UnitMessages.ModelRequired);
        }
    }
}