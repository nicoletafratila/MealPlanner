using FluentValidation;
using RecipeBook.Api.Features.ProductCategory.Resources;

namespace RecipeBook.Api.Features.ProductCategory.Commands.ResolveShare
{
    /// <summary>
    /// Validates resolve-share commands.
    /// </summary>
    public class ResolveShareCommandValidator : AbstractValidator<ResolveShareCommand>
    {
        public ResolveShareCommandValidator()
        {
            RuleFor(x => x.TargetUserId)
                .NotEmpty()
                .WithMessage(ProductCategoryMessages.UserIdRequired);
        }
    }
}
