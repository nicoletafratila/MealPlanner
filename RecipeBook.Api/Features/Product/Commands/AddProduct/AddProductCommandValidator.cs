using FluentValidation;

namespace RecipeBook.Api.Features.Product.Commands.AddProduct
{
    public class AddProductCommandValidator : AbstractValidator<AddProductCommand>
    {
        public AddProductCommandValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
            RuleFor(x => x.ImageContent).NotNull().NotEmpty();
            RuleFor(x => x.UnitId).GreaterThan(0);
            RuleFor(x => x.ProductCategoryId).GreaterThan(0);
        }
    }
}
