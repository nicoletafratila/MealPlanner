using FluentValidation;

namespace RecipeBook.Api.Features.Product.Commands.UpdateProduct
{
    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
