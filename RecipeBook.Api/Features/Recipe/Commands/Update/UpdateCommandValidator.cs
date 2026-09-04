using FluentValidation;
using RecipeBook.Api.Features.Recipe.Resources;
using RecipeBook.Shared.Resources;

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
                .WithMessage(RecipeMessages.ModelRequired);

            RuleFor(x => x.Model!.RecipeCategoryId)
                .NotEqual(Guid.Empty)
                .WithMessage(RecipeBookSharedMessages.RecipeCategoryRequired)
                .When(x => x.Model is not null);
        }
    }
}