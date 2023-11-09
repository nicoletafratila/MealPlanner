using FluentValidation;

namespace RecipeBook.Api.Features.Product.Commands.AddProduct
{
    public class AddProductCommandValidator : AbstractValidator<AddProductCommand>
    {
        public AddProductCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
