using FluentValidation;

namespace RecipeBook.Api.Features.Product.Queries.GetEditProduct
{
    public class GetEditProductQueryValidator : AbstractValidator<GetEditProductQuery>
    {
        public GetEditProductQueryValidator()
        {
            RuleFor(x => x.Id).NotNull().GreaterThan(0);
        }
    }
}
