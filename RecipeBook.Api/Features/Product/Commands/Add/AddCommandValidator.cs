using FluentValidation;
using RecipeBook.Api.Features.Product.Resources;
using RecipeBook.Shared.Resources;

namespace RecipeBook.Api.Features.Product.Commands.Add
{
    /// <summary>
    /// Validates product add commands.
    /// </summary>
    public class AddCommandValidator : AbstractValidator<AddCommand>
    {
        public AddCommandValidator()
        {
            RuleFor(x => x.Model)
                .NotNull()
                .WithMessage(ProductMessages.ModelRequired);

            RuleFor(x => x.Model!.ProductCategoryId)
                .NotEqual(Guid.Empty)
                .WithMessage(RecipeBookSharedMessages.ProductCategoryRequired)
                .When(x => x.Model is not null);
        }
    }
}