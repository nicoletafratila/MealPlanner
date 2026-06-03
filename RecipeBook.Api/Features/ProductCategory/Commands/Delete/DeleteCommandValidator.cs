using FluentValidation;
using RecipeBook.Api.Features.ProductCategory.Resources;

namespace RecipeBook.Api.Features.ProductCategory.Commands.Delete
{
    /// <summary>
    /// Validates product-category delete commands.
    /// </summary>
    public class DeleteCommandValidator : AbstractValidator<DeleteCommand>
    {
        public DeleteCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty)
                .WithMessage(ProductCategoryMessages.IdGreaterThanZero);
        }
    }
}