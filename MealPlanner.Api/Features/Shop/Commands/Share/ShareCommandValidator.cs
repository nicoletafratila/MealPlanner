using FluentValidation;

namespace MealPlanner.Api.Features.Shop.Commands.Share
{
    /// <summary>
    /// Validates shop share commands.
    /// </summary>
    public class ShareCommandValidator : AbstractValidator<ShareCommand>
    {
        public ShareCommandValidator()
        {
            RuleFor(x => x.ShopId)
                .NotEqual(Guid.Empty)
                .WithMessage(Resources.ShopMessages.IdGreaterThanZero);

            RuleFor(x => x.TargetUserId)
                .NotEmpty()
                .WithMessage(Resources.ShopMessages.UserIdRequired);
        }
    }
}
