using FluentValidation;

namespace RecipeBook.Api.Features.Product.Commands.Update
{
    /// <summary>
    /// Validates product update commands.
    /// </summary>
    public class UpdateCommandValidator : AbstractValidator<UpdateCommand>
    {
        public UpdateCommandValidator()
        {
            RuleFor(x => x.Model)
                .NotNull()
                .WithMessage("Model is required.");
        }
    }
}