using FluentValidation;

namespace Identity.Api.Features.ApplicationUser.Queries.GetEdit
{
    public class GetEditQueryValidator : AbstractValidator<GetEditQuery>
    {
        public GetEditQueryValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
        }
    }
}
