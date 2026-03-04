using FluentValidation;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Update
{
    /// <summary>
    /// Validates recipe-category update commands.
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