using FluentValidation;
using RecipeBook.Api.Features.RecipeCategory.Resources;

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
                .WithMessage(RecipeCategoryMessages.ModelsRequired)
                .NotEmpty()
                .WithMessage(RecipeCategoryMessages.ModelsNotEmpty);
        }
    }
}