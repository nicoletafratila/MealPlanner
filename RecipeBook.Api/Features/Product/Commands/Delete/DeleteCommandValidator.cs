using FluentValidation;

namespace RecipeBook.Api.Features.Product.Commands.Delete
{
    /// <summary>
    /// Validates product delete commands.
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