using FluentValidation;
using RecipeBook.Api.Features.RecipeCategory.Resources;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Delete
{
    /// <summary>
    /// Validates recipe-category delete commands.
    /// </summary>
    public class DeleteCommandValidator : AbstractValidator<DeleteCommand>
    {
        public DeleteCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty)
                .WithMessage(RecipeCategoryMessages.IdGreaterThanZero);
        }
    }
}