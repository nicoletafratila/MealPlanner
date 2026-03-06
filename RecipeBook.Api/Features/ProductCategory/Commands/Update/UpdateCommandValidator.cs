using FluentValidation;

namespace RecipeBook.Api.Features.ProductCategory.Commands.Update
{
    /// <summary>
    /// Validates product-category update commands.
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