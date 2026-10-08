using FluentValidation;
using RecipeBook.Api.Features.Recipe.Resources;

namespace RecipeBook.Api.Features.Recipe.Commands.ShareAll
{
    /// <summary>
    /// Validates share-all-recipes commands.
    /// </summary>
    public class ShareAllCommandValidator : AbstractValidator<ShareAllCommand>
    {
        public ShareAllCommandValidator()
        {
            RuleFor(x => x.TargetUserId)
                .NotEmpty()
                .WithMessage(RecipeMessages.UserIdRequired);
        }
    }
}
