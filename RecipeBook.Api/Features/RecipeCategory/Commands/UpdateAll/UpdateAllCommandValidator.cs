using FluentValidation;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.UpdateAll
{
    /// <summary>
    /// Validates bulk update commands for recipe categories.
    /// </summary>
    public class UpdateAllCommandValidator : AbstractValidator<UpdateAllCommand>
    {
        public UpdateAllCommandValidator()
        {
            RuleFor(x => x.Models)
                .NotNull()
                .WithMessage("Models collection is required.")
                .NotEmpty()
                .WithMessage("Models collection cannot be empty.");
        }
    }
}