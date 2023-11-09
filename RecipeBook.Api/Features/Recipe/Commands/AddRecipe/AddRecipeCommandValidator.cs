using FluentValidation;

namespace RecipeBook.Api.Features.Recipe.Commands.AddRecipe
{
    public class AddRecipeCommandValidator : AbstractValidator<AddRecipeCommand>
    {
        public AddRecipeCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
