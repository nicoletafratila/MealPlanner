using FluentValidation;

namespace MealPlanner.Api.Features.ProductCategory.Queries.GetEditProductCategory
{
    public class GetEditProductCategoryQueryValidator : AbstractValidator<GetEditProductCategoryQuery>
    {
        public GetEditProductCategoryQueryValidator()
        {
            RuleFor(x => x.Id).NotNull().GreaterThan(0);
        }
    }
}
