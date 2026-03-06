using FluentValidation;

namespace RecipeBook.Api.Features.Recipe.Commands.Add
{
    /// <summary>
    /// Validates add commands for recipes.
    /// </summary>
    public class AddCommandValidator : AbstractValidator<AddCommand>
    {
        public AddCommandValidator()
        {
            RuleFor(x => x.Model)
                .NotNull()
                .WithMessage("Model is required.");
        }
    }
}