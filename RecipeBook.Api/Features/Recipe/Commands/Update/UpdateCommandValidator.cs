using FluentValidation;

namespace RecipeBook.Api.Features.Recipe.Commands.Update
{
    /// <summary>
    /// Validates recipe update commands.
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