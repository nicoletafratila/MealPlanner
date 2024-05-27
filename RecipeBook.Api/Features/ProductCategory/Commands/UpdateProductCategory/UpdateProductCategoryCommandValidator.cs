using FluentValidation;

namespace MealPlanner.Api.Features.ProductCategory.Commands.UpdateProductCategory
{
    public class UpdateProductCategoryCommandValidator : AbstractValidator<UpdateProductCategoryCommand>
    {
        public UpdateProductCategoryCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
