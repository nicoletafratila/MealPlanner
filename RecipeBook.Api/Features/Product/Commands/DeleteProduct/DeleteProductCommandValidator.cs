using FluentValidation;

namespace RecipeBook.Api.Features.Product.Commands.DeleteProduct
{
    public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
    {
        public DeleteProductCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
