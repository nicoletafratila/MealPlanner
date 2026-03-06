using FluentValidation;

namespace RecipeBook.Api.Features.ProductCategory.Commands.Add
{
    /// <summary>
    /// Validates add commands for product categories.
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