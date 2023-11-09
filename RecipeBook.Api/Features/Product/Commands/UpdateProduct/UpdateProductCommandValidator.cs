using FluentValidation;

namespace RecipeBook.Api.Features.Product.Commands.UpdateProduct
{
    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0); 
            RuleFor(x => x.Name).NotNull().NotEmpty();
            RuleFor(x => x.ImageContent).NotNull().NotEmpty();
            RuleFor(x => x.UnitId).GreaterThan(0);
            RuleFor(x => x.ProductCategoryId).GreaterThan(0);
        }
    }
}
