using FluentValidation;
using RecipeBook.Api.Features.Product.Resources;

namespace RecipeBook.Api.Features.Product.Commands.Share
{
    /// <summary>
    /// Validates product share commands.
    /// </summary>
    public class ShareCommandValidator : AbstractValidator<ShareCommand>
    {
        public ShareCommandValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEqual(Guid.Empty)
                .WithMessage(ProductMessages.IdGreaterThanZero);

            RuleFor(x => x.TargetUserId)
                .NotEmpty()
                .WithMessage(ProductMessages.UserIdRequired);
        }
    }
}
