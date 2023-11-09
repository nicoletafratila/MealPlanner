using FluentValidation;

namespace RecipeBook.Api.Features.Recipe.Commands.UpdateRecipe
{
    public class UpdateRecipeCommandValidator : AbstractValidator<UpdateRecipeCommand>
    {
        public UpdateRecipeCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
