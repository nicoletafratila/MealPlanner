using FluentValidation;

namespace MealPlanner.Api.Features.ProductCategory.Commands.DeleteProductCategory
{
    public class DeleteProductCategoryCommandValidator : AbstractValidator<DeleteProductCategoryCommand>
    {
        public DeleteProductCategoryCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
