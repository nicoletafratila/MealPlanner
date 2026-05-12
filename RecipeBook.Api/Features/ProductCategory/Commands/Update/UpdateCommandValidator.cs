using FluentValidation;
using RecipeBook.Api.Features.ProductCategory.Resources;

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
                .WithMessage(ProductCategoryMessages.ModelRequired);
        }
    }
}