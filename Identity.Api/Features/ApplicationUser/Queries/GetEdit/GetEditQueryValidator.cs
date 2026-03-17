using FluentValidation;

namespace Identity.Api.Features.ApplicationUser.Queries.GetEdit
{
    /// <summary>
    /// Validates GetEditQuery for application users.
    /// </summary>
    public class GetEditQueryValidator : AbstractValidator<GetEditQuery>
    {
        public GetEditQueryValidator()
        {
            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty()
                .WithMessage("Name (username) is required.");
        }
    }
}