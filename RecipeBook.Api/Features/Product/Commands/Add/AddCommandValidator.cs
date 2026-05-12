using FluentValidation;
using RecipeBook.Api.Features.Product.Resources;

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
        }
    }
}