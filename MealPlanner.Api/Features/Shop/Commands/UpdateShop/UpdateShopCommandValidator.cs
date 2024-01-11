using FluentValidation;

namespace MealPlanner.Api.Features.Shop.Commands.UpdateShop
{
    public class UpdateShopCommandValidator : AbstractValidator<UpdateShopCommand>
    {
        public UpdateShopCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
