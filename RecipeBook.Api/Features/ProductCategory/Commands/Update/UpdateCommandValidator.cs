using FluentValidation;

namespace MealPlanner.Api.Features.ProductCategory.Commands.Update
{
    public class UpdateCommandValidator : AbstractValidator<UpdateCommand>
    {
        public UpdateCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
