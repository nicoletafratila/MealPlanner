using FluentValidation;

namespace MealPlanner.Api.Features.Shop.Commands.AddShop
{
    public class AddShopCommandValidator : AbstractValidator<AddShopCommand>
    {
        public AddShopCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
