using FluentValidation;
using MealPlanner.Api.Features.ProductCategory.Commands.ProductCategory;

namespace MealPlanner.Api.Features.ProductCategory.Commands.AddProductCategory
{
    public class AddProductCategoryCommandValidator : AbstractValidator<AddProductCategoryCommand>
    {
        public AddProductCategoryCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
