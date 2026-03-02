using FluentValidation;

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
                .WithMessage("Model is required.");
        }
    }
}