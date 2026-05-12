using FluentValidation;
using RecipeBook.Api.Features.Recipe.Resources;

namespace RecipeBook.Api.Features.Recipe.Commands.Delete
{
    /// <summary>
    /// Validates recipe delete commands.
    /// </summary>
    public class DeleteCommandValidator : AbstractValidator<DeleteCommand>
    {
        public DeleteCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage(RecipeMessages.IdGreaterThanZero);
        }
    }
}