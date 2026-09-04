using FluentValidation;
using RecipeBook.Api.Features.Recipe.Resources;
using RecipeBook.Shared.Resources;

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
                .WithMessage(RecipeMessages.ModelRequired);

            RuleFor(x => x.Model!.RecipeCategoryId)
                .NotEqual(Guid.Empty)
                .WithMessage(RecipeBookSharedMessages.RecipeCategoryRequired)
                .When(x => x.Model is not null);
        }
    }
}