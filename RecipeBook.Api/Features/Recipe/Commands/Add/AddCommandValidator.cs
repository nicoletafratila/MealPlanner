using FluentValidation;

namespace RecipeBook.Api.Features.Recipe.Commands.Add
{
    public class AddCommandValidator : AbstractValidator<AddCommand>
    {
        public AddCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
