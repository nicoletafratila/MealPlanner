using FluentValidation;

namespace MealPlanner.Api.Features.Shop.Commands.ShareAll
{
    /// <summary>
    /// Validates share-all-shops commands.
    /// </summary>
    public class ShareAllCommandValidator : AbstractValidator<ShareAllCommand>
    {
        public ShareAllCommandValidator()
        {
            RuleFor(x => x.TargetUserId)
                .NotEmpty()
                .WithMessage(Resources.ShopMessages.UserIdRequired);
        }
    }
}
