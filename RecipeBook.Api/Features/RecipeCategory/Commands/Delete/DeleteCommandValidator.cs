using FluentValidation;

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
                .GreaterThan(0)
                .WithMessage("Id must be greater than zero.");
        }
    }
}