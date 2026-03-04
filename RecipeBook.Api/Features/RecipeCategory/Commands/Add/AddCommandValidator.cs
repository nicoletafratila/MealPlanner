using FluentValidation;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Add
{
    /// <summary>
    /// Validates add commands for recipe categories.
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