using FluentValidation;

namespace RecipeBook.Api.Features.Recipe.Commands.DeleteRecipe
{
    public class DeleteRecipeCommandValidator : AbstractValidator<DeleteRecipeCommand>
    {
        public DeleteRecipeCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
